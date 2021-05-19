using SQLite;
using System.Collections.Generic;
using System.IO;

namespace Client_Android
{
    public class Settings
    {

        public string StoredRingtones = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "StoredRingtones.db3");
        public SQLiteConnection ringtonesMappingDB;
        public SocialAlarm_ClientAndroid socialAlarm;
        public List<Model_Ringtone> myRingtones { set; get; } = new List<Model_Ringtone>();

        public Settings(SocialAlarm_ClientAndroid socialAlarm)
        {
            this.socialAlarm = socialAlarm;
            ringtonesMappingDB = new SQLiteConnection(StoredRingtones);
            ringtonesMappingDB.CreateTable<Model_Ringtone>();
            myRingtones = ringtonesMappingDB.Table<Model_Ringtone>().ToList();
        }

        public void AddRingtone(Model_Ringtone ringtone)
        {
            myRingtones.Add(ringtone);
            if (ringtonesMappingDB.Find<Model_Ringtone>(element => ringtone.ID == element.ID) == null)
            {
                ringtonesMappingDB.Insert(ringtone);
            }
            else
            {
                ringtonesMappingDB.Update(ringtone);
            }

            socialAlarm.AddChangeRingtone(ringtone);
        }

        public void ReceiveRingtone(Model_Ringtone ringtone)
        {
            if (ringtonesMappingDB.Find<Model_Ringtone>(element => ringtone.ID == element.ID) == null)
            {
                ringtonesMappingDB.Insert(ringtone);
                myRingtones.Add(ringtone);
            }
            else
            {
                myRingtones[myRingtones.FindIndex(element => ringtone.ID == element.ID)] = ringtone;
                ringtonesMappingDB.Update(ringtone);
            }
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
            {
                socialAlarm.RemoveRingtone(ringtone);
            }
        }

        public void SyncRingtones()
        {
            List<Model_Ringtone> temp = ringtonesMappingDB.Table<Model_Ringtone>().ToList();
            foreach (Model_Ringtone ringtone in temp)
            {
                socialAlarm.CheckRingtone(ringtone);
            }
        }
    }
}