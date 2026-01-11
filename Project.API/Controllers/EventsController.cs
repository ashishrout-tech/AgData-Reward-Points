using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.DTOs.Event;
using Project.Application.Services;
using System.Security.Claims;

namespace Project.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly ILogger<EventsController> _logger;

        public EventsController(IEventService eventService, ILogger<EventsController> logger)
        {
            _eventService = eventService;
            _logger = logger;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? Guid.Parse(userIdClaim.Value) : Guid.Empty;
        }

        /// <summary>
        /// Create new event
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<EventDetailDto>> CreateEvent(
            [FromBody] CreateEventRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Creating event: {EventTitle}", request.Title);
                var userId = GetCurrentUserId();
                var eventDetail = await _eventService.CreateEventAsync(request, userId, cancellationToken);
                return CreatedAtAction(nameof(GetEvent), new { id = eventDetail.Id }, eventDetail);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid argument: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating event: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while creating the event" });
            }
        }

        /// <summary>
        /// Get event by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<EventDetailDto>> GetEvent(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving event: {EventId}", id);
                var eventDetail = await _eventService.GetEventAsync(id, cancellationToken);
                return Ok(eventDetail);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Event not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving event: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving the event" });
            }
        }

        /// <summary>
        /// Get all events
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<EventDto>>> GetAllEvents(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Retrieving all events - Skip: {Skip}, Take: {Take}", skip, take);
                var events = await _eventService.GetAllEventsAsync(skip, take, cancellationToken);
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving events: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving events" });
            }
        }

        /// <summary>
        /// Update event (organizer/admin only)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<EventDetailDto>> UpdateEvent(
            Guid id,
            [FromBody] UpdateEventRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Updating event: {EventId}", id);
                var eventDetail = await _eventService.UpdateEventAsync(id, request, cancellationToken);
                return Ok(eventDetail);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Event not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid argument: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating event: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while updating the event" });
            }
        }

        /// <summary>
        /// Cancel event (organizer/admin only)
        /// </summary>
        [HttpPost("{id}/cancel")]
        public async Task<ActionResult<EventDetailDto>> CancelEvent(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Cancelling event: {EventId}", id);
                var eventDetail = await _eventService.CancelEventAsync(id, cancellationToken);
                return Ok(eventDetail);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Event not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error cancelling event: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while cancelling the event" });
            }
        }

        /// <summary>
        /// Reschedule event (organizer/admin only)
        /// </summary>
        [HttpPost("{id}/reschedule")]
        public async Task<ActionResult<EventDetailDto>> RescheduleEvent(
            Guid id,
            [FromBody] RescheduleEventRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Rescheduling event: {EventId}", id);
                var eventDetail = await _eventService.RescheduleEventAsync(id, request, cancellationToken);
                return Ok(eventDetail);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Event not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid argument: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error rescheduling event: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while rescheduling the event" });
            }
        }

        /// <summary>
        /// Get event participants
        /// </summary>
        [HttpGet("{id}/participants")]
        public async Task<ActionResult<List<EventParticipantDto>>> GetEventParticipants(
            Guid id,
            [FromQuery] int? role = null,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting participants for event: {EventId}", id);
                var participants = await _eventService.GetEventParticipantsAsync(id, role, skip, take, cancellationToken);
                return Ok(participants);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Event not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving participants: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving participants" });
            }
        }

        /// <summary>
        /// Get participant details
        /// </summary>
        [HttpGet("{id}/participants/{userId}")]
        public async Task<ActionResult<EventParticipantDetailDto>> GetParticipantDetails(
            Guid id,
            Guid userId,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting participant details: {EventId}, {UserId}", id, userId);
                var participant = await _eventService.GetParticipantDetailsAsync(id, userId, cancellationToken);
                return Ok(participant);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving participant: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving participant" });
            }
        }

        /// <summary>
        /// Add participant to event (organizer/admin only)
        /// </summary>
        [HttpPost("{id}/participants")]
        public async Task<ActionResult<EventParticipantDto>> AddParticipant(
            Guid id,
            [FromBody] AddParticipantRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Adding participant to event: {EventId}", id);
                var participant = await _eventService.AddParticipantAsync(id, request, cancellationToken);
                return CreatedAtAction(nameof(GetParticipantDetails), new { id, userId = request.UserId }, participant);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error adding participant: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while adding participant" });
            }
        }

        /// <summary>
        /// Remove participant from event (organizer/admin only)
        /// </summary>
        [HttpDelete("{id}/participants/{userId}")]
        public async Task<IActionResult> RemoveParticipant(
            Guid id,
            Guid userId,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Removing participant from event: {EventId}, {UserId}", id, userId);
                await _eventService.RemoveParticipantAsync(id, userId, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error removing participant: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while removing participant" });
            }
        }

        /// <summary>
        /// Update participant role (organizer/admin only)
        /// </summary>
        [HttpPut("{id}/participants/{userId}/role")]
        public async Task<ActionResult<EventParticipantDto>> UpdateParticipantRole(
            Guid id,
            Guid userId,
            [FromBody] UpdateParticipantRoleRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Updating participant role: {EventId}, {UserId}", id, userId);
                var participant = await _eventService.UpdateParticipantRoleAsync(id, userId, request, cancellationToken);
                return Ok(participant);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating participant role: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while updating participant role" });
            }
        }

        /// <summary>
        /// Assign rank to participant (organizer/admin only)
        /// </summary>
        [HttpPut("{id}/participants/{userId}/rank")]
        public async Task<ActionResult<EventParticipantDto>> AssignParticipantRank(
            Guid id,
            Guid userId,
            [FromBody] AssignRankRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Assigning rank to participant: {EventId}, {UserId}", id, userId);
                var participant = await _eventService.AssignParticipantRankAsync(id, userId, request, cancellationToken);
                return Ok(participant);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid argument: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error assigning rank: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while assigning rank" });
            }
        }

        /// <summary>
        /// Search events
        /// </summary>
        [HttpGet("search/query")]
        public async Task<ActionResult<List<EventDto>>> SearchEvents(
            [FromQuery] string? title = null,
            [FromQuery] Guid? organizerId = null,
            [FromQuery] string? tag = null,
            [FromQuery] DateTime? startDateFrom = null,
            [FromQuery] DateTime? startDateTo = null,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Searching events - Title: {Title}, OrganizerId: {OrganizerId}", title, organizerId);
                var events = await _eventService.SearchEventsAsync(title, organizerId, tag, startDateFrom, startDateTo, skip, take, cancellationToken);
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error searching events: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while searching events" });
            }
        }

        /// <summary>
        /// Get events by organizer
        /// </summary>
        [HttpGet("organizer/{organizerId}")]
        public async Task<ActionResult<List<EventDto>>> GetEventsByOrganizer(
            Guid organizerId,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting events by organizer: {OrganizerId}", organizerId);
                var events = await _eventService.GetEventsByOrganizerAsync(organizerId, skip, take, cancellationToken);
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving organizer events: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving organizer events" });
            }
        }

        /// <summary>
        /// Get upcoming events
        /// </summary>
        [HttpGet("upcoming")]
        public async Task<ActionResult<List<EventDto>>> GetUpcomingEvents(
            [FromQuery] int daysAhead = 30,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting upcoming events - DaysAhead: {DaysAhead}", daysAhead);
                var events = await _eventService.GetUpcomingEventsAsync(daysAhead, skip, take, cancellationToken);
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving upcoming events: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving upcoming events" });
            }
        }

        /// <summary>
        /// Add tag to event (organizer/admin only)
        /// </summary>
        [HttpPost("{id}/tags")]
        public async Task<ActionResult<List<string>>> AddTag(
            Guid id,
            [FromBody] AddTagRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Adding tag to event: {EventId}", id);
                var tags = await _eventService.AddTagAsync(id, request, cancellationToken);
                return Ok(tags);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Event not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error adding tag: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while adding tag" });
            }
        }

        /// <summary>
        /// Remove tag from event (organizer/admin only)
        /// </summary>
        [HttpDelete("{id}/tags/{tag}")]
        public async Task<ActionResult<List<string>>> RemoveTag(
            Guid id,
            string tag,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Removing tag from event: {EventId}, Tag: {Tag}", id, tag);
                var tags = await _eventService.RemoveTagAsync(id, tag, cancellationToken);
                return Ok(tags);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error removing tag: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while removing tag" });
            }
        }

        /// <summary>
        /// Get event statistics
        /// </summary>
        [HttpGet("{id}/statistics")]
        public async Task<ActionResult<EventStatisticsDto>> GetEventStatistics(
            Guid id,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting event statistics: {EventId}", id);
                var stats = await _eventService.GetEventStatisticsAsync(id, cancellationToken);
                return Ok(stats);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Event not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving statistics: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving statistics" });
            }
        }

        /// <summary>
        /// Get user's upcoming events
        /// </summary>
        [HttpGet("user/{userId}/upcoming")]
        public async Task<ActionResult<List<EventDto>>> GetUserUpcomingEvents(
            Guid userId,
            [FromQuery] int? role = null,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting upcoming events for user: {UserId}", userId);
                var events = await _eventService.GetUserUpcomingEventsAsync(userId, role, skip, take, cancellationToken);
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving user upcoming events: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving user events" });
            }
        }

        /// <summary>
        /// Get user's event history
        /// </summary>
        [HttpGet("user/{userId}/history")]
        public async Task<ActionResult<List<EventDto>>> GetUserEventHistory(
            Guid userId,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting event history for user: {UserId}", userId);
                var events = await _eventService.GetUserEventHistoryAsync(userId, skip, take, cancellationToken);
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving user event history: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving event history" });
            }
        }
    }
}
