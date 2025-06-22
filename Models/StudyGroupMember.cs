using System.ComponentModel.DataAnnotations;

namespace StudyBuddyMobile.Models
{
    public class StudyGroupMember
    {
        [Key]
        public int Id { get; set; }

        public int StudyGroupId { get; set; }

        public int UserId { get; set; }

        public DateTime JoinedDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string Role { get; set; } = "Member"; // Creator, Admin, Member

        // Navigation properties
        public StudyGroup? StudyGroup { get; set; }
        public User? User { get; set; }
    }
}