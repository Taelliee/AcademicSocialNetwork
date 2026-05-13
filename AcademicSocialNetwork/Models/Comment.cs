using System.ComponentModel.DataAnnotations;

namespace AcademicSocialNetwork.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public bool IsEdited { get; set; }
        public bool IsDeleted { get; set; }

        // Foreign Keys
        [Required]
        public int UserId { get; set; }
        public User Author { get; set; }

        [Required]
        public int PostId { get; set; }
        public Post Post { get; set; }

        // For nested comments (replies)
        public int? ParentCommentId { get; set; }
        public Comment? ParentComment { get; set; }
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    }
}
