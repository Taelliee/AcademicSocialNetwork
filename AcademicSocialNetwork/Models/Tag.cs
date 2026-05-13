using System.ComponentModel.DataAnnotations;

namespace AcademicSocialNetwork.Models
{
    public class Tag
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public int UseCount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    }
}