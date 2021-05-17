using SQLite;
using System;

namespace Client_Android
{
    [Serializable]
    [Table("ringtones")]
    public class Model_Ringtone
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
            return "ID: " + ID + "; " +
                "User: " + User + "; " +
                "Ringtone name: " + RingtoneName + "; " +
                "Description: " + Description;
        }

        public Model_Ringtone()
        {
            Random rnd = new Random();

            ID = rnd.Next(100);
            User = "test";
            RingtoneName = "TestModel";
            Description = "Test for a RingtoneModel and RecyclerView. " +
                "Test for a RingtoneModel and RecyclerView. " +
                "Test for a RingtoneModel and RecyclerView. " +
                "Test for a RingtoneModel and RecyclerView. ";
        }
    }
}