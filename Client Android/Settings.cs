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
        public List<Ringtone> myRingtones { set; get; } = new List<Ringtone>();
        public Adapter_MyRingtones MyRingtonesAdapter { get; set; }

        public Settings(SocialAlarm_ClientAndroid socialAlarm)
        {
            this.socialAlarm = socialAlarm;
            ringtonesMappingDB = new SQLiteConnection(StoredRingtones);
            ringtonesMappingDB.CreateTable<Ringtone>();
            myRingtones = ringtonesMappingDB.Table<Ringtone>().ToList();
        }

        public void ReceiveRingtone(Ringtone ringtone)
        {
            if (ringtonesMappingDB.Find<Ringtone>(element => ringtone.ID == element.ID) == null)
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

        public void RemoveRingtone(Ringtone ringtone)
        {
            socialAlarm.RemoveRingtone(ringtone);
            ringtonesMappingDB.Delete<Ringtone>(ringtone.ID);
            myRingtones.Remove(ringtone);
        }

        public void CheckRingtone(Ringtone ringtone)
        {
            Ringtone temp = ringtonesMappingDB.Find<Ringtone>(element => ringtone.ID == element.ID);
            if (temp == null)
            { socialAlarm.RemoveRingtone(ringtone); }
        }

        public void SyncRingtones()
        {
            List<Ringtone> temp = ringtonesMappingDB.Table<Ringtone>().ToList();
            foreach (Ringtone ringtone in temp)
            { socialAlarm.CheckRingtone(ringtone); }
        }
    }
}