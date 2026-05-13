using System.ComponentModel.DataAnnotations;

namespace AcademicSocialNetwork.Models
{
    public class ConversationParticipant
    {
        public int Id { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastReadAt { get; set; }
        public bool HasLeft { get; set; }

        // Foreign Keys
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; }
    }
}