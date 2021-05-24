using System;

namespace Client_Android
{
    [Serializable]
    public class Model_AlarmLog
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
            return "ID: " + ID + "; " +
                "Who slept: " + UserSlept + "; " +
                "Who waked: " + UserWaker + "; " +
                "IsWaker: " + IsWaker + "; " +
                "Date time: " + DateTime + "; " +
                "Description: " + Description;
        }
    }
}