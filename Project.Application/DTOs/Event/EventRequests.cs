using System.ComponentModel.DataAnnotations;

namespace Project.Application.DTOs.Event
{
    public class CreateEventRequest
    {
        [Required(ErrorMessage = "Event title is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Event title must be between 2 and 200 characters")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Event description is required")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Event description must be between 10 and 1000 characters")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Start time is required")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        public DateTime EndTime { get; set; }

        public List<string>? Tags { get; set; }
    }

    public class UpdateEventRequest
    {
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Event title must be between 2 and 200 characters")]
        public string? Title { get; set; }

        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Event description must be between 10 and 1000 characters")]
        public string? Description { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }
    }

    public class RescheduleEventRequest
    {
        [Required(ErrorMessage = "New start time is required")]
        public DateTime NewStartTime { get; set; }

        [Required(ErrorMessage = "New end time is required")]
        public DateTime NewEndTime { get; set; }
    }

    public class AddParticipantRequest
    {
        [Required(ErrorMessage = "User ID is required")]
        public Guid UserId { get; set; }

        public int Role { get; set; } = 2;
    }

    public class UpdateParticipantRoleRequest
    {
        [Required(ErrorMessage = "Role is required")]
        public int Role { get; set; }
    }

    public class AssignRankRequest
    {
        [Required(ErrorMessage = "Rank is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Rank must be a positive integer")]
        public int Rank { get; set; }
    }

    public class AddTagRequest
    {
        [Required(ErrorMessage = "Tag is required")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Tag must be between 1 and 50 characters")]
        public string Tag { get; set; } = null!;
    }
}
