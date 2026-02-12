using Project.Application.DTOs.Event;

namespace Project.Application.Services
{
    public interface IEventService
    {
        // CRUD Operations
        Task<EventDetailDto> CreateEventAsync(CreateEventRequest request, Guid userId, CancellationToken cancellationToken = default);
        Task<EventDetailDto> GetEventAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<EventDetailDto>> GetAllEventsAsync(int skip = 0, int take = 10, CancellationToken cancellationToken = default);
        Task<EventDetailDto> UpdateEventAsync(Guid id, UpdateEventRequest request, CancellationToken cancellationToken = default);

        // Status Operations
        Task<EventDetailDto> CancelEventAsync(Guid id, CancellationToken cancellationToken = default);
        Task<EventDetailDto> RescheduleEventAsync(Guid id, RescheduleEventRequest request, CancellationToken cancellationToken = default);
        Task<List<EventParticipantDto>> GetEventParticipantsAsync(Guid id, int? role = null, int skip = 0, int take = 10, CancellationToken cancellationToken = default);

        // Participant Management
        Task<EventParticipantDto> AddParticipantAsync(Guid eventId, AddParticipantRequest request, CancellationToken cancellationToken = default);
        Task RemoveParticipantAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default);
        Task<EventParticipantDto> UpdateParticipantRoleAsync(Guid eventId, Guid userId, UpdateParticipantRoleRequest request, CancellationToken cancellationToken = default);
        Task<EventParticipantDto> AssignParticipantRankAsync(Guid eventId, Guid userId, AssignRankRequest request, CancellationToken cancellationToken = default);
        Task<EventParticipantDetailDto> GetParticipantDetailsAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default);

        // Search & Filter
        Task<List<EventDetailDto>> SearchEventsAsync(string? title, Guid? organizerId, string? tag, DateTime? startDateFrom, DateTime? startDateTo, int skip = 0, int take = 10, CancellationToken cancellationToken = default);
        Task<List<EventDetailDto>> GetEventsByOrganizerAsync(Guid organizerId, int skip = 0, int take = 10, CancellationToken cancellationToken = default);
        Task<List<EventDetailDto>> GetUpcomingEventsAsync(int skip = 0, int take = 10, CancellationToken cancellationToken = default);

        // Tags Management
        Task<List<string>> AddTagAsync(Guid id, AddTagRequest request, CancellationToken cancellationToken = default);
        Task<List<string>> RemoveTagAsync(Guid id, string tag, CancellationToken cancellationToken = default);

        // Statistics
        Task<EventStatisticsDto> GetEventStatisticsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<EventDto>> GetUserUpcomingEventsAsync(Guid userId, int? role = null, int skip = 0, int take = 10, CancellationToken cancellationToken = default);
        Task<List<EventDto>> GetUserEventHistoryAsync(Guid userId, int skip = 0, int take = 10, CancellationToken cancellationToken = default);
    }
}
