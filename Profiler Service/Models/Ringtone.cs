using System.ComponentModel.DataAnnotations;

namespace Profiler_Service.Models
{
    public class Ringtone
    {
        [Key]
        public int ID { set; get; }

        [Required]
        public string User { set; get; }

        [Required]
        public string RingtoneName { set; get; }

        public string Description { set; get; }

        [Required]
        public string File { set; get; }

        public override string ToString()
        {
            return "ID: " + ID + "; " +
                "User: " + User + "; " +
                "Ringtone name: " + RingtoneName + "; " +
                "Description: " + Description;
        }
    }
}