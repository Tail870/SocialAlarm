using System;
using System.ComponentModel.DataAnnotations;

namespace DataModels
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
            return "ID: " + ID + "; " +
                "User: " + User + "; " +
                "IsWaker: " + IsWaker + "; " +
                "Time: " + Time + "; " +
                "Threshold: " + Threshold + "; " +
                "Description: " + Description;
        }
    }
}