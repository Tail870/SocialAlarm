using System.ComponentModel.DataAnnotations;

namespace Profiler_Service.Models
{
    public class Alarm
    {
        [Key]
        public int ID { set; get; }

        [Required]
        public string User { set; get; }

        [Required]
        public bool IsWaker { set; get; }

        [Required]
        public DateTimeOffset Time { set; get; }

        [Required]
        public int Threshold { set; get; }

        public string Description { set; get; }

        public override string ToString()
        {
            string result = string.Empty;
            if (ID != null)
                result += ("ID: " + ID.ToString() + "; ");
            else
                result += ("ID: [null]; ");
            if (User != null)
                result += ("User: " + User.ToString() + "; ");
            else
                result += ("User: [null]; ");
            if (IsWaker != null)
                result += ("For wake: " + IsWaker.ToString() + "; ");
            else
                result += ("For wake: [null]; ");
            if (Time != null)
                result += ("Time: " + Time.ToString() + "; ");
            else
                result += ("Time: [null]; ");
            if (Threshold != null)
                result += ("Threshold: " + Threshold.ToString() + "; ");
            else
                result += ("Threshold: [null]; ");
            if (Description != null)
                result += ("Description: " + Description.ToString() + ". ");
            else
                result += ("Description: [null]. ");
            return result;
        }
    }
}