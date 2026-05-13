using System.ComponentModel.DataAnnotations;

namespace AcademicSocialNetwork.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }

        public string? AttachmentUrl { get; set; }
        public string? AttachmentType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsDeleted { get; set; }

        // Foreign Keys
        [Required]
        public int SenderId { get; set; }
        public User Sender { get; set; }

        [Required]
        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; }
    }
}
