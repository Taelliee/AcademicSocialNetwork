using System.ComponentModel.DataAnnotations;

namespace AcademicSocialNetwork.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FullName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    [StringLength(100)]
    public string? Major { get; set; }

    public uint? ClassYear { get; set; }

    [StringLength(500)]
    public string? Bio { get; set; }

    public string? ProfileImageUrl { get; set; }
    public string? CoverImageUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastActive { get; set; }
    public bool IsOnline { get; set; }
    public bool IsAdmin { get; set; }

    // Navigation properties
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Like> Likes { get; set; } = new List<Like>();

    public ICollection<Connection> Followers { get; set; } = new List<Connection>();
    public ICollection<Connection> Following { get; set; } = new List<Connection>();

    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();

    public ICollection<GroupMember> GroupMemberships { get; set; } = new List<GroupMember>();
    public ICollection<EventAttendee> EventAttendances { get; set; } = new List<EventAttendee>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
