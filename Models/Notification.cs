using System.ComponentModel.DataAnnotations;

namespace VisualHead.Models
{
    public class Notification
    {
        public string message { get; set; }
        public string Nid { get; set; }
        [Key]
        public int Id { get; set; }
    }
}
