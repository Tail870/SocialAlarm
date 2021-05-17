using System;
using System.ComponentModel.DataAnnotations;

namespace DataModels
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
            return "ID: " + ID + "; " +
                "Who slept: " + UserSlept + "; " +
                "Who waked: " + UserWaker + "; " +
                "IsWaker: " + IsWaker + "; " +
                "Date time: " + DateTime + "; " +
                "Description: " + Description;
        }
    }
}