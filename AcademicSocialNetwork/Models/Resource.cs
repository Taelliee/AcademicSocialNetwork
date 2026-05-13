using System.ComponentModel.DataAnnotations;

namespace AcademicSocialNetwork.Models
{
    public enum ResourceType
    {
        Document,
        Video,
        Link,
        Other
    }

    public class Resource
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required]
        public string Url { get; set; }

        public ResourceType Type { get; set; }

        [StringLength(100)]
        public string? Subject { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        [Required]
        public int UploadedById { get; set; }
        public User UploadedBy { get; set; }

        public int? GroupId { get; set; }
        public Group? Group { get; set; }
    }
}