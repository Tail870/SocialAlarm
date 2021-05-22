using SQLite;
using System.Collections.Generic;
using System.IO;

namespace Client_Android
{
    public class Settings
    {
        private readonly string StoredRingtones = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "StoredRingtones.db3");
        public SQLiteConnection ringtonesMappingDB;
        public SocialAlarm_ClientAndroid socialAlarm;
        public List<Model_Ringtone> myRingtones { set; get; } = new List<Model_Ringtone>();
        public Adapter_MyRingtones MyRingtonesAdapter { get; set; }

        public Settings(SocialAlarm_ClientAndroid socialAlarm)
        {
            this.socialAlarm = socialAlarm;
            ringtonesMappingDB = new SQLiteConnection(StoredRingtones);
            ringtonesMappingDB.CreateTable<Model_Ringtone>();
            myRingtones = ringtonesMappingDB.Table<Model_Ringtone>().ToList();
        }

        public void ReceiveRingtone(Model_Ringtone ringtone)
        {
            if (ringtonesMappingDB.Find<Model_Ringtone>(element => ringtone.ID == element.ID) == null)
            {
                ringtonesMappingDB.Insert(ringtone);
                myRingtones.Add(ringtone);
                MyRingtonesAdapter.NotifyDataSetChanged();
            }
            else
            {
                ringtonesMappingDB.Update(ringtone);
                myRingtones[myRingtones.FindIndex(element => ringtone.ID == element.ID)] = ringtone;
            }
            MyRingtonesAdapter.NotifyDataSetChanged();
        }

        public void RemoveRingtone(Model_Ringtone ringtone)
        {
            socialAlarm.RemoveRingtone(ringtone);
            ringtonesMappingDB.Delete<Model_Ringtone>(ringtone.ID);
            myRingtones.Remove(ringtone);
        }

        public void CheckRingtone(Model_Ringtone ringtone)
        {
            Model_Ringtone temp = ringtonesMappingDB.Find<Model_Ringtone>(element => ringtone.ID == element.ID);
            if (temp == null)
            { socialAlarm.RemoveRingtone(ringtone); }
        }

        public void SyncRingtones()
        {
            List<Model_Ringtone> temp = ringtonesMappingDB.Table<Model_Ringtone>().ToList();
            foreach (Model_Ringtone ringtone in temp)
            { socialAlarm.CheckRingtone(ringtone); }
        }
    }
}