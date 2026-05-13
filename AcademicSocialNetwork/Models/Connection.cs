using System.ComponentModel.DataAnnotations;

namespace AcademicSocialNetwork.Models
{
    public enum ConnectionStatus
    {
        Pending,
        Accepted,
        Rejected,
        Blocked
    }

    public class Connection
    {
        public int Id { get; set; }

        public ConnectionStatus Status { get; set; } = ConnectionStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? AcceptedAt { get; set; }

        // Foreign Keys
        [Required]
        public int FollowerId { get; set; }
        public User Follower { get; set; }

        [Required]
        public int FollowingId { get; set; }
        public User Following { get; set; }
    }
}