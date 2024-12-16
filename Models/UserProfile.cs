using System.ComponentModel.DataAnnotations;
using VisualHead.Areas.Identity.Data;

namespace VisualHead.Models
{
    public class UserProfile
    {
        [Key]
        [Required]
        public string UniqueUserName { get; set; }
        [Required]
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public string Type { get; set; }
        public DateTime Month { get; set; }
    }
}
