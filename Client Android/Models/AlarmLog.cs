using System;

namespace Client_Android
{
    [Serializable]
    public class AlarmLog
    {
        public int ID { set; get; }

        public string UserSlept { set; get; }
        public string DisplayedNameSlept { set; get; }

        public string UserWaker { set; get; }
        public string DisplayedNameWaker { set; get; }

        public string Description { set; get; }

        public DateTimeOffset DateTime { set; get; }

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