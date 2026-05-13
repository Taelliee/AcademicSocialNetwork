using System.ComponentModel.DataAnnotations;

namespace AcademicSocialNetwork.Models
{
    public enum AttendanceStatus
    {
        Going,
        Maybe,
        NotGoing
    }

    public class EventAttendee
    {
        public int Id { get; set; }

        public AttendanceStatus Status { get; set; } = AttendanceStatus.Going;
        public DateTime RespondedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        public int EventId { get; set; }
        public Event Event { get; set; }
    }
}