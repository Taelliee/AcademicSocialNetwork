using System.ComponentModel.DataAnnotations;

namespace AcademicSocialNetwork.Models
{
    public class Conversation
    {
        public int Id { get; set; }

        public string? Name { get; set; } // For group conversations
        public bool IsGroup { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastMessageAt { get; set; }

        // Navigation properties
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
    }
}