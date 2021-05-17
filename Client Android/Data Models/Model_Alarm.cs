using System;

namespace Client_Android
{
    [Serializable]
    public class Model_Alarm
    {
        public int ID { set; get; }

        public string User { set; get; }
        public string DisplayedName { set; get; }
        public string UserDecription { set; get; }

        public bool IsWaker { set; get; }

        public DateTimeOffset Time { set; get; }

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