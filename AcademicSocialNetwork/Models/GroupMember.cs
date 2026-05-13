using System.ComponentModel.DataAnnotations;

namespace AcademicSocialNetwork.Models
{
    public enum GroupRole
    {
        Member,
        Moderator,
        Admin
    }

    public class GroupMember
    {
        public int Id { get; set; }

        public GroupRole Role { get; set; } = GroupRole.Member;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        public int GroupId { get; set; }
        public Group Group { get; set; }
    }
}