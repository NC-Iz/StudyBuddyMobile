using System.ComponentModel.DataAnnotations;

namespace StudyBuddyMobile.Models
{
    public class StudyResource
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(100)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string ResourceType { get; set; } = string.Empty; // PDF, Video, Document

        public string? FileName { get; set; }

        public byte[]? FileContent { get; set; }

        public string? ContentType { get; set; }

        public long FileSize { get; set; } = 0;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Foreign Key
        public int UserId { get; set; }

        // Navigation property
        public User? User { get; set; }
    }
}