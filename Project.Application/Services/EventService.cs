using AutoMapper;
using Microsoft.Extensions.Logging;
using Project.Application.DTOs.Event;
using Project.Domain.Entities.Event;
using Project.Domain.Enums;
using Project.Domain.Interfaces;

namespace Project.Application.Services
{
    public class EventService : IEventService
    {
        private readonly IEventAsyncRepository _eventRepository;
        private readonly IUserAsyncRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<EventService> _logger;

        public EventService(
            IEventAsyncRepository eventRepository,
            IUserAsyncRepository userRepository,
            IMapper mapper,
            ILogger<EventService> logger)
        {
            _eventRepository = eventRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<EventDetailDto> CreateEventAsync(CreateEventRequest request, Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Creating event: {EventTitle}", request.Title);

                if (request.StartTime >= request.EndTime)
                {
                    throw new ArgumentException("Start time must be before end time.");
                }

                var organizer = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (organizer == null)
                {
                    throw new KeyNotFoundException($"User with ID '{userId}' not found");
                }

                var eventEntity = new Event(request.Title, request.Description, null!, null!);
                
                var metadata = new EventMetadata(organizer);
                
                if (request.Tags?.Any() == true)
                {
                    foreach (var tag in request.Tags)
                    {
                        metadata.AddTag(tag);
                    }
                }

                var schedule = new EventSchedule(request.StartTime, request.EndTime);

                eventEntity.SetMetadata(metadata);
                eventEntity.SetSchedule(schedule);

                var organizerParticipant = new EventParticipant(eventEntity.Id, userId, ParticipantRole.Organizer);
                eventEntity.Participants.Add(organizerParticipant);

                var createdEvent = await _eventRepository.AddAsync(eventEntity, cancellationToken);

                _logger.LogInformation("Event created successfully: {EventId}", createdEvent.Id);

                return MapToEventDetailDto(createdEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating event: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<EventDetailDto> GetEventAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting event: {EventId}", id);

            var eventEntity = await _eventRepository.GetByIdAsync(id, cancellationToken);
            if (eventEntity == null)
            {
                throw new KeyNotFoundException($"Event with ID '{id}' not found");
            }

            return MapToEventDetailDto(eventEntity);
        }

        public async Task<List<EventDto>> GetAllEventsAsync(int skip = 0, int take = 10, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all events - Skip: {Skip}, Take: {Take}", skip, take);

            var events = await _eventRepository.GetAllAsync(skip, take, cancellationToken);

            return _mapper.Map<List<EventDto>>(events);
        }

        public async Task<EventDetailDto> UpdateEventAsync(Guid id, UpdateEventRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating event: {EventId}", id);

                var eventEntity = await _eventRepository.GetByIdAsync(id, cancellationToken);
                if (eventEntity == null)
                {
                    throw new KeyNotFoundException($"Event with ID '{id}' not found");
                }

                if (eventEntity.IsCancelled)
                {
                    throw new InvalidOperationException("Cannot update a cancelled event.");
                }

                if (request.StartTime.HasValue && request.EndTime.HasValue)
                {
                    if (request.StartTime >= request.EndTime)
                    {
                        throw new ArgumentException("Start time must be before end time.");
                    }
                    eventEntity.EventSchedule.Reschedule(request.StartTime.Value, request.EndTime.Value);
                }

                await _eventRepository.UpdateAsync(eventEntity, cancellationToken);

                _logger.LogInformation("Event updated successfully: {EventId}", id);

                return MapToEventDetailDto(eventEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating event: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<EventDetailDto> CancelEventAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Cancelling event: {EventId}", id);

                var eventEntity = await _eventRepository.GetByIdAsync(id, cancellationToken);
                if (eventEntity == null)
                {
                    throw new KeyNotFoundException($"Event with ID '{id}' not found");
                }

                if (eventEntity.IsCancelled)
                {
                    throw new InvalidOperationException("Event is already cancelled.");
                }

                eventEntity.CancelEvent();
                await _eventRepository.UpdateAsync(eventEntity, cancellationToken);

                _logger.LogInformation("Event cancelled successfully: {EventId}", id);

                return MapToEventDetailDto(eventEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error cancelling event: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<EventDetailDto> RescheduleEventAsync(Guid id, RescheduleEventRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Rescheduling event: {EventId}", id);

                var eventEntity = await _eventRepository.GetByIdAsync(id, cancellationToken);
                if (eventEntity == null)
                {
                    throw new KeyNotFoundException($"Event with ID '{id}' not found");
                }

                if (eventEntity.IsCancelled)
                {
                    throw new InvalidOperationException("Cannot reschedule a cancelled event.");
                }

                eventEntity.EventSchedule.Reschedule(request.NewStartTime, request.NewEndTime);
                await _eventRepository.UpdateAsync(eventEntity, cancellationToken);

                _logger.LogInformation("Event rescheduled successfully: {EventId}", id);

                return MapToEventDetailDto(eventEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error rescheduling event: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<List<EventParticipantDto>> GetEventParticipantsAsync(Guid id, int? role = null, int skip = 0, int take = 10, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting participants for event: {EventId}, Role: {Role}", id, role);

            var eventExists = await _eventRepository.GetByIdAsync(id, cancellationToken);
            if (eventExists == null)
            {
                throw new KeyNotFoundException($"Event with ID '{id}' not found");
            }

            var participants = await _eventRepository.GetEventParticipantsAsync(id, role, skip, take, cancellationToken);

            return _mapper.Map<List<EventParticipantDto>>(participants);
        }

        public async Task<EventParticipantDto> AddParticipantAsync(Guid eventId, AddParticipantRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Adding participant to event: {EventId}, UserId: {UserId}", eventId, request.UserId);

                var eventEntity = await _eventRepository.GetByIdAsync(eventId, cancellationToken);
                if (eventEntity == null)
                {
                    throw new KeyNotFoundException($"Event with ID '{eventId}' not found");
                }

                if (eventEntity.IsCancelled)
                {
                    throw new InvalidOperationException("Cannot add participants to a cancelled event.");
                }

                var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID '{request.UserId}' not found");
                }

                var role = (ParticipantRole)request.Role;
                var participant = new EventParticipant(eventId, request.UserId, role);

                eventEntity.AddParticipant(participant);
                await _eventRepository.UpdateAsync(eventEntity, cancellationToken);

                _logger.LogInformation("Participant added successfully: {EventId}, {UserId}", eventId, request.UserId);

                return _mapper.Map<EventParticipantDto>(participant);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error adding participant: {Message}", ex.Message);
                throw;
            }
        }

        public async Task RemoveParticipantAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Removing participant from event: {EventId}, UserId: {UserId}", eventId, userId);

                var eventEntity = await _eventRepository.GetByIdAsync(eventId, cancellationToken);
                if (eventEntity == null)
                {
                    throw new KeyNotFoundException($"Event with ID '{eventId}' not found");
                }

                eventEntity.RemoveParticipant(userId);
                await _eventRepository.UpdateAsync(eventEntity, cancellationToken);

                _logger.LogInformation("Participant removed successfully: {EventId}, {UserId}", eventId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error removing participant: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<EventParticipantDto> UpdateParticipantRoleAsync(Guid eventId, Guid userId, UpdateParticipantRoleRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating participant role: {EventId}, UserId: {UserId}, Role: {Role}", eventId, userId, request.Role);

                var eventEntity = await _eventRepository.GetByIdAsync(eventId, cancellationToken);
                if (eventEntity == null)
                {
                    throw new KeyNotFoundException($"Event with ID '{eventId}' not found");
                }

                var participant = eventEntity.Participants.FirstOrDefault(p => p.UserId == userId);
                if (participant == null)
                {
                    throw new KeyNotFoundException($"Participant not found in event");
                }

                var role = (ParticipantRole)request.Role;
                participant.UpdateRole(role);
                
                await _eventRepository.UpdateAsync(eventEntity, cancellationToken);

                _logger.LogInformation("Participant role updated successfully: {EventId}, {UserId}", eventId, userId);

                return _mapper.Map<EventParticipantDto>(participant);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating participant role: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<EventParticipantDto> AssignParticipantRankAsync(Guid eventId, Guid userId, AssignRankRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Assigning rank to participant: {EventId}, UserId: {UserId}, Rank: {Rank}", eventId, userId, request.Rank);

                var eventEntity = await _eventRepository.GetByIdAsync(eventId, cancellationToken);
                if (eventEntity == null)
                {
                    throw new KeyNotFoundException($"Event with ID '{eventId}' not found");
                }

                var participant = eventEntity.Participants.FirstOrDefault(p => p.UserId == userId);
                if (participant == null)
                {
                    throw new KeyNotFoundException($"Participant not found in event");
                }

                participant.AssignRank(request.Rank);
                await _eventRepository.UpdateAsync(eventEntity, cancellationToken);

                _logger.LogInformation("Rank assigned successfully: {EventId}, UserId: {UserId}, Rank: {Rank}", eventId, userId, request.Rank);

                return _mapper.Map<EventParticipantDto>(participant);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error assigning rank: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<EventParticipantDetailDto> GetParticipantDetailsAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting participant details: {EventId}, UserId: {UserId}", eventId, userId);

            var eventEntity = await _eventRepository.GetByIdAsync(eventId, cancellationToken);
            if (eventEntity == null)
            {
                throw new KeyNotFoundException($"Event with ID '{eventId}' not found");
            }

            var participant = eventEntity.Participants.FirstOrDefault(p => p.UserId == userId);
            if (participant == null)
            {
                throw new KeyNotFoundException($"Participant not found in event");
            }

            return _mapper.Map<EventParticipantDetailDto>(participant);
        }

        public async Task<List<EventDto>> SearchEventsAsync(string? title, Guid? organizerId, string? tag, DateTime? startDateFrom, DateTime? startDateTo, int skip = 0, int take = 10, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Searching events - Title: {Title}, OrganizerId: {OrganizerId}, Tag: {Tag}", title, organizerId, tag);

            var events = await _eventRepository.SearchAsync(title, organizerId, tag, startDateFrom, startDateTo, skip, take, cancellationToken);

            return _mapper.Map<List<EventDto>>(events);
        }

        public async Task<List<EventDto>> GetEventsByOrganizerAsync(Guid organizerId, int skip = 0, int take = 10, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting events by organizer: {OrganizerId}", organizerId);

            var events = await _eventRepository.GetByOrganizerAsync(organizerId, skip, take, cancellationToken);

            return _mapper.Map<List<EventDto>>(events);
        }

        public async Task<List<EventDto>> GetUpcomingEventsAsync(int daysAhead = 30, int skip = 0, int take = 10, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting upcoming events - DaysAhead: {DaysAhead}", daysAhead);

            var events = await _eventRepository.GetUpcomingAsync(DateTime.UtcNow, daysAhead, skip, take, cancellationToken);

            return _mapper.Map<List<EventDto>>(events);
        }

        public async Task<List<string>> AddTagAsync(Guid id, AddTagRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Adding tag to event: {EventId}, Tag: {Tag}", id, request.Tag);

                var eventEntity = await _eventRepository.GetByIdAsync(id, cancellationToken);
                if (eventEntity == null)
                {
                    throw new KeyNotFoundException($"Event with ID '{id}' not found");
                }

                eventEntity.EventMetadata.AddTag(request.Tag);
                await _eventRepository.UpdateAsync(eventEntity, cancellationToken);

                _logger.LogInformation("Tag added successfully: {EventId}", id);

                return eventEntity.EventMetadata.Tags;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error adding tag: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<List<string>> RemoveTagAsync(Guid id, string tag, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Removing tag from event: {EventId}, Tag: {Tag}", id, tag);

                var eventEntity = await _eventRepository.GetByIdAsync(id, cancellationToken);
                if (eventEntity == null)
                {
                    throw new KeyNotFoundException($"Event with ID '{id}' not found");
                }

                if (!eventEntity.EventMetadata.Tags.Contains(tag))
                {
                    throw new KeyNotFoundException($"Tag '{tag}' not found");
                }

                eventEntity.EventMetadata.Tags.Remove(tag);
                await _eventRepository.UpdateAsync(eventEntity, cancellationToken);

                _logger.LogInformation("Tag removed successfully: {EventId}", id);

                return eventEntity.EventMetadata.Tags;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error removing tag: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<EventStatisticsDto> GetEventStatisticsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting event statistics: {EventId}", id);

            var eventEntity = await _eventRepository.GetByIdAsync(id, cancellationToken);
            if (eventEntity == null)
            {
                throw new KeyNotFoundException($"Event with ID '{id}' not found");
            }

            var organizersCount = eventEntity.Participants.Count(p => p.Role == ParticipantRole.Organizer);
            var speakersCount = eventEntity.Participants.Count(p => p.Role == ParticipantRole.Speaker);
            var attendeesCount = eventEntity.Participants.Count(p => p.Role == ParticipantRole.Attendee);

            return new EventStatisticsDto
            {
                EventId = id,
                TotalParticipants = eventEntity.Participants.Count,
                OrgainzersCount = organizersCount,
                SpeakersCount = speakersCount,
                AttendeesCount = attendeesCount,
                DurationInMinutes = eventEntity.EventSchedule.DurationInMinutes(),
                StartTime = eventEntity.EventSchedule.StartTime,
                EndTime = eventEntity.EventSchedule.EndTime,
                IsCancelled = eventEntity.IsCancelled
            };
        }

        public async Task<List<EventDto>> GetUserUpcomingEventsAsync(Guid userId, int? role = null, int skip = 0, int take = 10, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting upcoming events for user: {UserId}, Role: {Role}", userId, role);

            var events = await _eventRepository.GetUserParticipationAsync(userId, includeHistory: false, skip, take, cancellationToken);

            if (role.HasValue)
            {
                events = events
                    .Where(e => e.Participants.Any(p => p.UserId == userId && (int)p.Role == role.Value))
                    .ToList();
            }

            return _mapper.Map<List<EventDto>>(events);
        }

        public async Task<List<EventDto>> GetUserEventHistoryAsync(Guid userId, int skip = 0, int take = 10, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting event history for user: {UserId}", userId);

            var events = await _eventRepository.GetUserParticipationAsync(userId, includeHistory: true, skip, take, cancellationToken);

            return _mapper.Map<List<EventDto>>(events);
        }

        private EventDetailDto MapToEventDetailDto(Event eventEntity)
        {
            var duration = eventEntity.EventSchedule.DurationInMinutes();
            var organizerId = eventEntity.EventMetadata.OrganizerId;
            var organizerName = eventEntity.EventMetadata.Organizer?.Name ?? "Unknown";

            return new EventDetailDto
            {
                Id = eventEntity.Id,
                Title = eventEntity.Title,
                Description = "N/A",
                StartTime = eventEntity.EventSchedule.StartTime,
                EndTime = eventEntity.EventSchedule.EndTime,
                DurationInMinutes = duration,
                IsCancelled = eventEntity.IsCancelled,
                OrganizerId = organizerId,
                OrganizerName = organizerName,
                Tags = eventEntity.EventMetadata.Tags,
                ParticipantCount = eventEntity.Participants.Count,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}
