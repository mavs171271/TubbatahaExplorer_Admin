using System.ComponentModel.DataAnnotations;

namespace VisualHead.Models
{
    public class FeedbackData
    {
        public string Types { get; set; }
        public string Body { get; set; }
        public DateTime DateCreated { get; set; }
        public string UserId { get; set; }
        [Key]
        public int Rfid { get; set; }
    }
}
