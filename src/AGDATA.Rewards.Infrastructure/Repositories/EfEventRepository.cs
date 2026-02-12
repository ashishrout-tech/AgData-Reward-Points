using Microsoft.EntityFrameworkCore;
using Project.Domain.Entities.Event;
using Project.Domain.Interfaces;
using Project.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Infrastructure.Repositories
{
    public class EfEventRepository : IEventAsyncRepository
    {
        private readonly AppDbContext _db;

        public EfEventRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Event> AddAsync(Event eventEntity, CancellationToken cancellationToken = default)
        {
            await _db.Events.AddAsync(eventEntity, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return eventEntity;
        }

        public async Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default, Boolean? track = true)
        {
            var eventQuery = _db.Events.AsQueryable();
            if(track.HasValue && !track.Value)
            {
                eventQuery = eventQuery.AsNoTracking();
			}
			return await eventQuery
                .Include(e => e.EventMetadata)
                    .ThenInclude(em => em.Organizer)
                .Include(e => e.EventSchedule)
				.Include(e => e.Participants)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<List<Event>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Events
                .AsNoTracking()
                .Include(e => e.EventMetadata)
                    .ThenInclude(em => em.Organizer)
                .Include(e => e.EventSchedule)
				.Include(e => e.Participants)
                .Where(e => !e.IsCancelled)
                .OrderBy(e => e.EventSchedule.StartTime)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Event>> GetAllAsync(int skip, int take, CancellationToken cancellationToken = default)
        {
            return await _db.Events
                .AsNoTracking()
                .Include(e => e.EventMetadata)
                    .ThenInclude(em => em.Organizer)
                .Include(e => e.EventSchedule)
				.Include(e => e.Participants)
                .Where(e => !e.IsCancelled)
                .OrderBy(e => e.EventSchedule.StartTime)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task UpdateAsync(Event eventEntity, CancellationToken cancellationToken = default)
        {
			await _db.SaveChangesAsync(cancellationToken);
		}

        public async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var eventEntity = await _db.Events.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
            if (eventEntity == null)
                throw new KeyNotFoundException("Event not found.");

            _db.Events.Remove(eventEntity);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Event>> SearchAsync(string? title, Guid? organizerId, string? tag, DateTime? startDateFrom, DateTime? startDateTo, CancellationToken cancellationToken = default)
        {
            var query = _db.Events.AsNoTracking()
                .Include(e => e.EventMetadata)
                    .ThenInclude(em => em.Organizer)
                .Include(e => e.EventSchedule)
				.Include(e => e.Participants)
                .Where(e => !e.IsCancelled);

            if (!string.IsNullOrWhiteSpace(title))
            {
				query = query.Where(e => EF.Functions.Like(e.Title, $"%{title}%"));
			}

            if (organizerId.HasValue && organizerId != Guid.Empty)
            {
                query = query.Where(e => e.EventMetadata.OrganizerId == organizerId);
            }

            if (!string.IsNullOrWhiteSpace(tag))
            {
                query = query.Where(e => e.EventMetadata.Tags.Contains(tag));
            }

            if (startDateFrom.HasValue)
            {
                query = query.Where(e => e.EventSchedule.StartTime >= startDateFrom);
            }

            if (startDateTo.HasValue)
            {
                query = query.Where(e => e.EventSchedule.StartTime <= startDateTo);
            }

            return await query
                .OrderBy(e => e.EventSchedule.StartTime)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Event>> SearchAsync(string? title, Guid? organizerId, string? tag, DateTime? startDateFrom, DateTime? startDateTo, int skip, int take, CancellationToken cancellationToken = default)
        {
            var query = _db.Events.AsNoTracking()
                .Include(e => e.EventMetadata)
                    .ThenInclude(em => em.Organizer)
                .Include(e => e.EventSchedule)
				.Include(e => e.Participants)
                .Where(e => !e.IsCancelled);

            if (!string.IsNullOrWhiteSpace(title))
            {
				query = query.Where(e => EF.Functions.Like(e.Title, $"%{title}%"));
			}

            if (organizerId.HasValue && organizerId != Guid.Empty)
            {
                query = query.Where(e => EF.Functions.Like(e.Title, $"%{title}%"));
            }

            if (!string.IsNullOrWhiteSpace(tag))
            {
                query = query.Where(e => e.EventMetadata.Tags.Contains(tag));
            }

            if (startDateFrom.HasValue)
            {
                query = query.Where(e => e.EventSchedule.StartTime >= startDateFrom);
            }

            if (startDateTo.HasValue)
            {
                query = query.Where(e => e.EventSchedule.StartTime <= startDateTo);
            }

            return await query
                .OrderBy(e => e.EventSchedule.StartTime)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Event>> GetByOrganizerAsync(Guid organizerId, CancellationToken cancellationToken = default)
        {
            return await _db.Events
                .AsNoTracking()
                .Include(e => e.EventMetadata)
                    .ThenInclude(em => em.Organizer)
                .Include(e => e.EventSchedule)
				.Include(e => e.Participants)
                .Where(e => e.EventMetadata.OrganizerId == organizerId && !e.IsCancelled)
                .OrderBy(e => e.EventSchedule.StartTime)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Event>> GetByOrganizerAsync(Guid organizerId, int skip, int take, CancellationToken cancellationToken = default)
        {
            return await _db.Events
                .AsNoTracking()
                .Include(e => e.EventMetadata)
                .ThenInclude(em => em.Organizer)
                .Include(e => e.EventSchedule)
				.Include(e => e.Participants)
                .Where(e => e.EventMetadata.OrganizerId == organizerId && !e.IsCancelled)
                .OrderBy(e => e.EventSchedule.StartTime)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Event>> GetUpcomingAsync(DateTime fromDate, int daysAhead = 30, CancellationToken cancellationToken = default)
        {
            var toDate = fromDate.AddDays(daysAhead);

            return await _db.Events
                .AsNoTracking()
                .Include(e => e.EventMetadata)
                .ThenInclude(em => em.Organizer)
                .Include(e => e.EventSchedule)
				.Include(e => e.Participants)
                .Where(e => e.EventSchedule.StartTime >= fromDate && e.EventSchedule.StartTime <= toDate && !e.IsCancelled)
                .OrderBy(e => e.EventSchedule.StartTime)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Event>> GetUpcomingAsync(DateTime fromDate, int skip, int take, CancellationToken cancellationToken = default)
        {

            return await _db.Events
                .AsNoTracking()
                .Include(e => e.EventMetadata)
                .ThenInclude(em => em.Organizer)
                .Include(e => e.EventSchedule)
				.Include(e => e.Participants)
                .Where(e => e.EventSchedule.EndTime >= fromDate && !e.IsCancelled)
                .OrderBy(e => e.EventSchedule.StartTime)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Event>> GetUserParticipationAsync(Guid userId, bool includeHistory = false, CancellationToken cancellationToken = default)
        {
            var query = _db.Events.AsNoTracking()
                .Include(e => e.EventMetadata)
                .ThenInclude(em => em.Organizer)
                .Include(e => e.EventSchedule)
				.Include(e => e.Participants)
                .Where(e => e.Participants.Any(p => p.UserId == userId) && !e.IsCancelled);

            if (!includeHistory)
            {
                // Only upcoming events
                query = query.Where(e => e.EventSchedule.StartTime >= DateTime.UtcNow)
                    .OrderBy(e => e.EventSchedule.StartTime);
            }
            else
            {
                // History - past events
                query = query.OrderByDescending(e => e.EventSchedule.EndTime);
            }

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<List<Event>> GetUserParticipationAsync(Guid userId, bool includeHistory, int skip, int take, CancellationToken cancellationToken = default)
        {
            var query = _db.Events.AsNoTracking()
                .Include(e => e.EventMetadata)
                .ThenInclude(em => em.Organizer)
                .Include(e => e.EventSchedule)
				.Include(e => e.Participants)
                .Where(e => e.Participants.Any(p => p.UserId == userId) && !e.IsCancelled);

            if (!includeHistory)
            {
                // Only upcoming events
                query = query.Where(e => e.EventSchedule.StartTime >= DateTime.UtcNow);
                return await query
                    .OrderBy(e => e.EventSchedule.StartTime)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync(cancellationToken);
            }
            else
            {
                // History - past events
                return await query
                    .Where(e => DateTime.UtcNow > e.EventSchedule.EndTime)
                    .OrderByDescending(e => e.EventSchedule.EndTime)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync(cancellationToken);
            }
        }

        public async Task<List<EventParticipant>> GetEventParticipantsAsync(Guid eventId, int? role = null, int skip = 0, int take = 10, CancellationToken cancellationToken = default)
        {
            var query = _db.EventParticipants.AsNoTracking()
                .Include(p => p.User)
				.Where(p => p.EventId == eventId);


			if (role.HasValue)
            {
                query = query.Where(p => (int)p.Role == role.Value);
            }

            return await query
                .OrderBy(p => p.JoinedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<EventParticipant?> GetParticipantAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default)
        {
            return await _db.EventParticipants.AsNoTracking()
                .Include(p => p.User)
				.FirstOrDefaultAsync(p => p.EventId == eventId && p.UserId == userId, cancellationToken);
        }

		public async Task<EventParticipant> AddParticipantAsync(EventParticipant participant, CancellationToken cancellationToken = default)
		{
			await _db.EventParticipants.AddAsync(participant, cancellationToken);
			await _db.SaveChangesAsync();

			await _db.Entry(participant).Reference(p => p.User).LoadAsync(cancellationToken);
			return participant;
		}

        public async Task<bool> RemoveParticipantAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default)
        {
            var participant = await _db.EventParticipants
                .FirstOrDefaultAsync(p => p.EventId == eventId && p.UserId == userId, cancellationToken);
            if (participant == null)
                return false;
            _db.EventParticipants.Remove(participant);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
		}

        public async Task<EventParticipant> UpdateParticipantAsync(EventParticipant participant, CancellationToken cancellationToken = default)
        {
            var existing = await _db.EventParticipants
                .FirstOrDefaultAsync(p => p.EventId == participant.EventId && p.UserId == participant.UserId, cancellationToken);
            if (existing == null)
                throw new KeyNotFoundException("Participant not found.");
            _db.Entry(existing).CurrentValues.SetValues(participant);
            await _db.SaveChangesAsync(cancellationToken);
            await _db.Entry(existing).Reference(p => p.User).LoadAsync(cancellationToken);
            return existing;
		}
	}
}
