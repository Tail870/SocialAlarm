using SQLite;
using System;

namespace Client_Android
{
    [Serializable]
    [Table("ringtones")]
    public class Ringtone
    {
        [PrimaryKey]
        public int ID { set; get; }

        public string User { set; get; }
        public string DisplayedUserName { set; get; }

        public string RingtoneName { set; get; }

        public string Description { set; get; }

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