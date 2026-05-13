using System.ComponentModel.DataAnnotations;

namespace AcademicSocialNetwork.Models
{
    public class Group
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsPrivate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        [Required]
        public int CreatorId { get; set; }
        public User Creator { get; set; }

        // Navigation properties
        public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
        public ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}