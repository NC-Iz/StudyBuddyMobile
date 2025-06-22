using System.ComponentModel.DataAnnotations;

namespace StudyBuddyMobile.Models
{
    public class GroupMessage
    {
        [Key]
        public int Id { get; set; }

        public int StudyGroupId { get; set; }

        public int UserId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        public DateTime SentDate { get; set; } = DateTime.Now;

        // Navigation properties
        public StudyGroup? StudyGroup { get; set; }
        public User? User { get; set; }
    }
}