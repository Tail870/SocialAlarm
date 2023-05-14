using System;

namespace Client_Android
{
    [Serializable]
    public class Alarm
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