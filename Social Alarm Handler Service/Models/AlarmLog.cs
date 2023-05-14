using System;
using System.ComponentModel.DataAnnotations;

namespace Social_Alarm.Models
{
    public class AlarmLog
    {
        [Key]
        public int ID { set; get; }

        [Required]
        public string UserSlept { set; get; }

        public string UserWaker { set; get; }

        public string Description { set; get; }

        [Required]
        public DateTimeOffset DateTime { set; get; }

        [Required]
        public bool IsWaker { set; get; }

        public override string ToString()
        {
            string result = string.Empty;
            if (ID != null)
                result += ("ID: " + ID.ToString() + "; ");
            else
                result += ("ID: [null]; ");
            if (UserSlept != null)
                result += ("Who slept: " + UserSlept.ToString() + "; ");
            else
                result += ("Who slept: [null]; ");
            if (UserWaker != null)
                result += ("Who waked: " + UserWaker.ToString() + "; ");
            else
                result += ("Who waked: [null]; ");
            if (UserWaker != null)
                result += ("IsWaker: " + UserWaker.ToString() + "; ");
            else
                result += ("IsWaker: [null]; ");
            if (DateTime != null)
                result += ("\"Date time: " + DateTime.ToString() + "; ");
            else
                result += ("Date time: [null]; ");
            if (Description != null)
                result += ("Description: " + Description.ToString() + ". ");
            else
                result += ("Description: [null]. ");
            return result;
        }
    }
}