using System.ComponentModel.DataAnnotations;

namespace AcademicSocialNetwork.Models
{
    public enum NotificationType
    {
        Like,
        Comment,
        Follow,
        Message,
        EventInvite,
        GroupInvite,
        Mention
    }

    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        public string Content { get; set; }

        public string? LinkUrl { get; set; }

        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }

        // Foreign Keys
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        public int? ActorId { get; set; }
        public User? Actor { get; set; }

        public int? PostId { get; set; }
        public Post? Post { get; set; }
    }
}