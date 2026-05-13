using System.ComponentModel.DataAnnotations;

namespace AcademicSocialNetwork.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        public string? ImageUrl { get; set; }
        public string? LinkUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public bool IsEdited { get; set; }
        public bool IsDeleted { get; set; }

        // Foreign Keys
        [Required]
        public int UserId { get; set; }
        public User Author { get; set; }

        public int? GroupId { get; set; }
        public Group? Group { get; set; }

        // Navigation properties
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();

        // Computed properties
        public int LikeCount => Likes?.Count(l => !l.IsDeleted) ?? 0;
        public int CommentCount => Comments?.Count(c => !c.IsDeleted) ?? 0;
    }
}
