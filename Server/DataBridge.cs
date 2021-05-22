using DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Linq;

namespace Social_Alarm_Server
{
    public class DataBridge
    {
        public class DataBaseContext : DbContext
        {
            public DbSet<User> Users { get; set; }
            public DbSet<Alarm> Alarms { get; set; }
            public DbSet<AlarmLog> AlarmLogs { get; set; }
            public DbSet<Ringtone> Ringtones { get; set; }

            public DataBaseContext()
            {
                Database.EnsureCreated();
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                //TODO Config DB.
                optionsBuilder.UseNpgsql(
                    "Host=localhost;" +
                    "Port=5432;" +
                    "Database=social_alarm;" +
                    "Username=postgres;" +
                    "Password=socialalarm");
            }
        }

        // This method used to check user's credentials.
        //TODO encrypt.
        public bool AuthCheck(string username, string password)
        {
            DataBaseContext context = new();
            User ConnectingUser = context.Users.Find(username);
            if (ConnectingUser != null)
            {
                Console.WriteLine("User: " + ConnectingUser.DisplayedName + " (login: " + ConnectingUser.Login + ")");
                if (ConnectingUser.Password == password)
                {
                    Console.WriteLine(username + " - correct password.");
                    return true;
                }
                else
                { Console.WriteLine(username + " - wrong password."); }
            }
            else
            { Console.WriteLine("No such user: " + username); }
            return false;
        }

        // This method used to get displayed username
        public string GetDisplayedName(string username)
        {
            DataBaseContext context = new();
            User ConnectingUser = context.Users.Find(username);
            if (ConnectingUser == null)
            { return null; }
            return ConnectingUser.DisplayedName;
        }

        public string GetUserDescription(string username)
        {
            DataBaseContext context = new();
            User user = context.Users.Find(username);
            if (user == null)
            { return null; }
            return user.Contacts;
        }

        public Alarm AddAlarm(Alarm addedAlarm)
        {
            if (addedAlarm.Threshold > 1440)
            { addedAlarm.Threshold = 1440; }
            // HACK Bug: if Date is too low, then DB probably receives wrong offset in addedAlarm.Time.
            DateTimeOffset CurrentDay = DateTimeOffset.Now;
            DateTimeOffset AlarmTime = addedAlarm.Time;
            addedAlarm.Time = new DateTimeOffset(CurrentDay.Year, CurrentDay.Month, CurrentDay.Day, AlarmTime.Hour, AlarmTime.Minute, 0, 0, AlarmTime.Offset);

            try
            {
                DataBaseContext context = new();
                if (context.Alarms.Where(element => element.ID == addedAlarm.ID).Count() > 0)
                { context.Alarms.Update(addedAlarm); }
                else
                { context.Alarms.Add(addedAlarm); }
                if (addedAlarm.ID == 0)
                { Console.WriteLine("Added alarm: " + addedAlarm.ToString()); }
                else
                { Console.WriteLine("Moded alarm: " + addedAlarm.ToString()); }

                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("/------------AddAlarm------------\\");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("\\------------AddAlarm------------/");
                return null;
            }
            return addedAlarm;
        }

        public IQueryable<Alarm> GetAlarms(string user, bool owner)
        {
            DataBaseContext context = new();
            if (user.Length > 0)
            {
                if (owner)
                { return context.Alarms.Where(alarm => alarm.User == user); }
                else
                { return context.Alarms.Where(alarm => alarm.User != user); }
            }
            else
            { return context.Alarms; }
        }

        public Alarm GetAlarm(int id)
        {
            try
            {
                DataBaseContext context = new();
                Alarm alarm = context.Alarms.Where(element => element.ID == id).First();
                if (alarm.Threshold > 1440)
                { alarm.Threshold = 1440; }
                return alarm;
            }
            catch (Exception ex)
            {
                Console.WriteLine("/------------GetAlarm------------\\");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("\\------------GetAlarm------------/");
                return null;
            }
        }

        public bool RemoveAlarm(Alarm alarm)
        {
            try
            {
                DataBaseContext context = new();
                context.Alarms.Remove(alarm);
                Console.WriteLine("Removed alarm:" + alarm.ToString());
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("/------------RemoveAlarm------------\\");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("\\------------RemoveAlarm------------/");
                return false;
            }
            return true;
        }

        public Ringtone AddRingtone(Ringtone ringtone)
        {
            EntityEntry<Ringtone> addedRingtone = null;
            try
            {
                DataBaseContext context = new();
                if (context.Ringtones.Where(element => element.ID == ringtone.ID).Count() > 0)
                { addedRingtone = context.Ringtones.Update(ringtone); }
                else
                { addedRingtone = context.Ringtones.Add(ringtone); }

                if (ringtone.ID == 0)
                { Console.WriteLine("Added ringtone: " + ringtone.ToString()); }
                else
                { Console.WriteLine("Moded ringtone: " + ringtone.ToString()); }

                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("/------------AddRingtone------------\\");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("\\------------AddRingtone------------/");
                return null;
            }
            return addedRingtone.Entity;
        }

        public IQueryable<Ringtone> GetRingtones(string Owner)
        {
            DataBaseContext context = new();
            return context.Ringtones.Where(Ringtone => Ringtone.User == Owner);
        }

        public bool CheckRingtone(Ringtone ringtone)
        {
            DataBaseContext context = new();
            if (context.Ringtones.Where(element => element == ringtone).Count() > 0)
            { return true; }
            else
            { return false; }
        }

        public bool RemoveRingtone(Ringtone ringtone)
        {
            try
            {
                DataBaseContext context = new();
                context.Ringtones.Remove(ringtone);
                Console.WriteLine("Removed ringtone:" + ringtone.ToString());
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("/------------RemoveRingtone------------\\");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("\\------------RemoveRingtone------------/");
                return false;
            }
            return true;
        }

        public IQueryable<AlarmLog> GetAlarmLogs(string Participator)
        {
            DataBaseContext context = new();
            return context.AlarmLogs.Where(AlarmLog => (AlarmLog.UserSlept == Participator) || (AlarmLog.UserWaker == Participator));
        }

        public AlarmLog AlarmToLog(Alarm alarm, DateTimeOffset WokeUpTime, string Waker)
        {
            AlarmLog alarmLog = new();
            alarmLog.UserSlept = alarm.User;
            alarmLog.UserWaker = Waker;
            alarmLog.Description = alarm.Description;
            alarmLog.DateTime = WokeUpTime;
            alarmLog.IsWaker = alarm.IsWaker;
            return alarmLog;
        }

        public AlarmLog AddAlarmLog(Alarm alarm, DateTimeOffset WokeUpTime, string Waker)
        {
            try
            {
                DataBaseContext context = new();
                AlarmLog addedLog = context.AlarmLogs.Add(AlarmToLog(alarm, WokeUpTime, Waker)).Entity;
                context.SaveChanges();
                Console.WriteLine("Added alarm log: " + addedLog.ToString());
                return addedLog;
            }
            catch (Exception ex)
            { Console.WriteLine(ex.ToString()); }
            return null;
        }
    }
}