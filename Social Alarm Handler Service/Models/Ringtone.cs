using System.ComponentModel.DataAnnotations;

namespace Social_Alarm.Models
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
            string result = string.Empty;
            if (ID != null)
                result += ("ID: " + ID + "; ");
            else
                result += ("ID: [null]; ");
            if (User != null)
                result += ("User: " + User + "; ");
            else
                result += ("User: [null]; ");
            if (RingtoneName != null)
                result += ("Ringtone name: " + RingtoneName + "; ");
            else
                result += ("Ringtone name: [null]; ");
            if (Description != null)
                result += ("Description: " + Description + ". ");
            else
                result += ("Description: [null]. ");
            return result;
        }
    }
}