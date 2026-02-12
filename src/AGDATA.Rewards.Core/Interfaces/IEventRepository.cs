using Project.Domain.Entities.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Interfaces
{
    public interface IEventAsyncRepository
    {
        Task<Event> AddAsync(Event eventEntity, CancellationToken cancellationToken = default);
        Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default, Boolean? track = true);
        Task<List<Event>> GetAllAsync(CancellationToken cancellationToken = default);
        Task UpdateAsync(Event eventEntity, CancellationToken cancellationToken = default);
        Task RemoveAsync(Guid id, CancellationToken cancellationToken = default);
        
        // Search & Filter with IQueryable
        Task<List<Event>> SearchAsync(string? title, Guid? organizerId, string? tag, DateTime? startDateFrom, DateTime? startDateTo, CancellationToken cancellationToken = default);
        Task<List<Event>> GetByOrganizerAsync(Guid organizerId, CancellationToken cancellationToken = default);
        Task<List<Event>> GetUpcomingAsync(DateTime fromDate, int daysAhead = 30, CancellationToken cancellationToken = default);
        Task<List<Event>> GetUserParticipationAsync(Guid userId, bool includeHistory = false, CancellationToken cancellationToken = default);
        
        // Paginated queries (database-level)
        Task<List<Event>> GetAllAsync(int skip, int take, CancellationToken cancellationToken = default);
        Task<List<Event>> SearchAsync(string? title, Guid? organizerId, string? tag, DateTime? startDateFrom, DateTime? startDateTo, int skip, int take, CancellationToken cancellationToken = default);
        Task<List<Event>> GetByOrganizerAsync(Guid organizerId, int skip, int take, CancellationToken cancellationToken = default);
        Task<List<Event>> GetUpcomingAsync(DateTime fromDate, int skip, int take, CancellationToken cancellationToken = default);
        Task<List<Event>> GetUserParticipationAsync(Guid userId, bool includeHistory, int skip, int take, CancellationToken cancellationToken = default);
        
        // Participant queries
        Task<List<EventParticipant>> GetEventParticipantsAsync(Guid eventId, int? role = null, int skip = 0, int take = 10, CancellationToken cancellationToken = default);
        Task<EventParticipant?> GetParticipantAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default);
		Task<EventParticipant> AddParticipantAsync(EventParticipant participant, CancellationToken cancellationToken = default);
        Task<bool> RemoveParticipantAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default);
        Task<EventParticipant> UpdateParticipantAsync(EventParticipant participant, CancellationToken cancellationToken = default);
	}
}
