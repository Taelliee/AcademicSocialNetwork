using System.ComponentModel.DataAnnotations;

namespace AcademicSocialNetwork.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public string? ImageUrl { get; set; }
        public string? Location { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        [Required]
        public int OrganizerId { get; set; }
        public User Organizer { get; set; }

        public int? GroupId { get; set; }
        public Group? Group { get; set; }

        // Navigation properties
        public ICollection<EventAttendee> Attendees { get; set; } = new List<EventAttendee>();
    }
}