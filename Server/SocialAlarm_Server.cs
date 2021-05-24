using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Social_Alarm_Server
{
    [Authorize]
    public class SocialAlarm_Server : Hub
    {
        private readonly DataBridgeSocialAlarm dataBridge = new();
        private static readonly Dictionary<string, HubCallerContext> usersContexts = new();
        private readonly int DelayTime = Configs.DelayTime;

        public override async Task OnConnectedAsync()
        {
            string user = Context.User.Identity.Name;
            if (user.Length > 0)
            {
                if (usersContexts.ContainsKey(user))
                {
                    Console.WriteLine(user + " already connected! Closing existing connection...");
                    usersContexts[user].Abort();
                    usersContexts[user] = Context;
                    await Clients.Caller.SendAsync("ServerNotification", "connected");
                }
                else
                {
                    usersContexts.Add(user, Context);
                    Console.WriteLine(user + " connected!");
                    Console.Write("All connected users: ");
                    foreach (KeyValuePair<string, HubCallerContext> pair in usersContexts)
                    { Console.Write("[" + pair.Key + "] "); }
                    Console.WriteLine();
                    await Clients.Caller.SendAsync("ServerNotification", "reconnected");
                }
            }
            else
            { Context.Abort(); }
        }

        public override Task OnDisconnectedAsync(Exception ex)
        {
            if (usersContexts.TryGetValue(Context.User.Identity.Name, out HubCallerContext temp))
            {
                if (Context.ConnectionId == temp.ConnectionId)
                { usersContexts.Remove(temp.User.Identity.Name); }
            }
            Console.Write(Context.User.Identity.Name + " disconnected! ");
            if (usersContexts.Count > 0)
            {
                Console.Write("List of connected users: ");
                foreach (KeyValuePair<string, HubCallerContext> pair in usersContexts)
                { Console.Write("[" + pair.Key + "]"); }
            }
            else
            { Console.WriteLine("No users connected."); }

            return Task.CompletedTask;
        }

        [HubMethodName("GetAlarms")]
        public async Task GetMyAlarms(bool owner)
        {
            System.Linq.IQueryable<Alarm> alarms = dataBridge.GetAlarms(Context.User.Identity.Name, owner);
            foreach (Alarm alarm in alarms)
            {
                await Clients.Caller.SendAsync("GetAlarm", alarm, dataBridge.GetDisplayedName(alarm.User), dataBridge.GetUserDescription(alarm.User));
                Thread.Sleep(DelayTime);
            }
        }

        [HubMethodName("AddAlarm")]
        public async Task AddAlarm(Alarm alarm)
        {
            alarm.User = Context.User.Identity.Name;
            if (dataBridge.AddAlarm(alarm) != null)
            {
                await Clients.All.SendAsync("ServerNotification", "Refresh, new alarm added/moded: " + alarm.ToString());
                await Clients.All.SendAsync("GetAlarm", alarm, dataBridge.GetDisplayedName(alarm.User), dataBridge.GetUserDescription(alarm.User));
            }
            else
            { await Clients.Caller.SendAsync("ServerNotification", "Error adding alarm."); }
        }

        [HubMethodName("RemoveAlarm")]
        public async Task RemoveAlarm(Alarm alarm)
        {
            if (alarm.User == Context.User.Identity.Name)
            {
                if (dataBridge.RemoveAlarm(alarm))
                { await Clients.All.SendAsync("RemovedAlarm", alarm); }
                else
                { await Clients.Caller.SendAsync("ServerNotification", "Error removing alarm."); }
            }
        }

        [HubMethodName("GetAlarmLogs")]
        public async Task SendAlarmsLogs()
        {
            foreach (AlarmLog alarmLog in dataBridge.GetAlarmLogs(Context.User.Identity.Name))
            {
                if (alarmLog.UserSlept == Context.User.Identity.Name || alarmLog.UserWaker == Context.User.Identity.Name)
                {
                    await Clients.Caller.SendAsync("GetAlarmLogs", alarmLog, dataBridge.GetDisplayedName(alarmLog.UserWaker), dataBridge.GetDisplayedName(alarmLog.UserSlept));
                    Thread.Sleep(DelayTime);
                }
            }
        }

        [HubMethodName("RingAlarm")]
        public async Task PrepareToRingAlarm(int alarmID, int ringtoneID)
        {
            Alarm alarm = dataBridge.GetAlarm(alarmID);
            alarm.Time = alarm.Time.AddYears(1).ToLocalTime();
            DateTimeOffset currentTime = new DateTimeOffset(2, 1, 2, DateTimeOffset.Now.Hour, DateTimeOffset.Now.Minute, DateTimeOffset.Now.Second, DateTimeOffset.Now.Offset);
            Console.WriteLine(Context.User.Identity.Name + " atempted to ring alarm of user " + alarm.User + " With ringtone ID=" + ringtoneID.ToString() + ". Alarm:\n" + alarm.ToString() + "\nTime marks (threshold, begin alarm, current time, exact alarm):\n" + "Threshold (min.): " + alarm.Threshold.ToString() + ", " + alarm.Time.AddMinutes(0 - alarm.Threshold).TimeOfDay.ToString() + " < " + currentTime.TimeOfDay.ToString() + " < " + alarm.Time.TimeOfDay.ToString());
            if (currentTime.AddMinutes(0 - alarm.Threshold) <= alarm.Time)
            {
                string ringer = dataBridge.GetDisplayedName(Context.User.Identity.Name);
                if (ringer == null && ringer.Length == 0)
                { ringer = Context.User.Identity.Name; }
                if (usersContexts.ContainsKey(alarm.User))
                { await Clients.User(usersContexts[alarm.User].UserIdentifier).SendAsync("RingAlarm", ringtoneID, alarmID, ringer); }
                else
                { await Clients.Caller.SendAsync("ServerNotification", "user_not_online"); }
            }
            else
            { await Clients.Caller.SendAsync("ServerNotification", "wrong_alarm_time"); }
        }

        [HubMethodName("IamAwake")]
        public async Task CreateAlarmLog(int alarmID, string Waker)
        {
            Alarm alarm = dataBridge.GetAlarm(alarmID);
            if (alarm.User == Context.User.Identity.Name)
            {
                AlarmLog newLog = dataBridge.AddAlarmLog(alarm, DateTimeOffset.Now, Waker);
                if (newLog != null)
                {
                    await Clients.User(usersContexts[newLog.UserWaker].UserIdentifier).SendAsync("GetAlarmLogs", newLog, dataBridge.GetDisplayedName(newLog.UserWaker), dataBridge.GetDisplayedName(newLog.UserSlept));
                    await Clients.User(usersContexts[newLog.UserWaker].UserIdentifier).SendAsync("GetAlarmLogs", newLog, dataBridge.GetDisplayedName(newLog.UserWaker), dataBridge.GetDisplayedName(newLog.UserSlept));
                }
            }
        }

        [HubMethodName("FinishAlarm")]
        public async Task FinishAlarm(int alarmID)
        {
            Alarm alarm = dataBridge.GetAlarm(alarmID);
            alarm.Time = alarm.Time.AddYears(1).ToLocalTime();
            DateTimeOffset currentTime = new DateTimeOffset(2, 1, 2, DateTimeOffset.Now.Hour, DateTimeOffset.Now.Minute, DateTimeOffset.Now.Second, DateTimeOffset.Now.Offset);
            Console.WriteLine(Context.User.Identity.Name + " atempted to finish alarm of user " + alarm.User + ". Alarm:\n" + alarm.ToString() +
                "\nTime marks (threshold, begin alarm, current time, exact alarm):\n" + "Threshold (min.): " + alarm.Threshold.ToString() + ", " +
                alarm.Time.AddMinutes(0 - alarm.Threshold).TimeOfDay.ToString() + " < " + currentTime.TimeOfDay.ToString() + " < " + alarm.Time.TimeOfDay.ToString());
            if ((alarm.Time.AddMinutes(0 - alarm.Threshold) <= currentTime) && (currentTime <= alarm.Time))
            {
                string ringerDisplay = dataBridge.GetDisplayedName(Context.User.Identity.Name);
                if (ringerDisplay == null && ringerDisplay.Length == 0)
                { ringerDisplay = Context.User.Identity.Name; }
                if (!usersContexts.ContainsKey(alarm.User))
                { await Clients.Caller.SendAsync("ServerNotification", "user_not_online"); }
                else
                { await Clients.User(usersContexts[alarm.User].UserIdentifier).SendAsync("FinishAlarm", alarmID, Context.User.Identity.Name, ringerDisplay); }
            }
            else
            { await Clients.Caller.SendAsync("ServerNotification", "wrong_alarm_time"); }
        }

        [HubMethodName("GetRingtones")]
        public async Task SendRingtones(Alarm userToAnnoy)
        {
            Ringtone defaultRingtone = new Ringtone
            {
                ID = 0,
                User = userToAnnoy.User,
                RingtoneName = "¯\\_(ツ)_/¯",
                Description = "¯\\_(ツ)_/¯"
            };
            await Clients.Caller.SendAsync("GetRingtones", defaultRingtone, userToAnnoy.ID);
            Thread.Sleep(DelayTime);
            foreach (Ringtone ringtone in dataBridge.GetRingtones(userToAnnoy.User))
            {
                await Clients.Caller.SendAsync("GetRingtones", ringtone, userToAnnoy.ID);
                Thread.Sleep(DelayTime);
            }
        }

        [HubMethodName("GetMyRingtone")]
        public async Task SendPersonalRingtones()
        {
            foreach (Ringtone ringtone in dataBridge.GetRingtones(Context.User.Identity.Name))
            {
                await Clients.Caller.SendAsync("GetMyRingtone", ringtone);
                Thread.Sleep(DelayTime);
            }
        }

        [HubMethodName("AddRingtone")]
        public async Task AddRingtone(Ringtone ringtone)
        {
            ringtone.User = Context.User.Identity.Name;
            ringtone = dataBridge.AddRingtone(ringtone);
            if (ringtone != null)
            {
                await Clients.Caller.SendAsync("ServerNotification", "Added ringtone: " + ringtone.ToString());
                await Clients.Caller.SendAsync("GetMyRingtone", ringtone);
            }
            else
            { await Clients.Caller.SendAsync("ServerNotification", "Error adding ringtone."); }
        }

        [HubMethodName("CheckRingtone")]
        public async Task CheckRingtone(Ringtone ringtone)
        {
            if (ringtone.User == Context.User.Identity.Name)
            {
                if (dataBridge.CheckRingtone(ringtone))
                { await Clients.Caller.SendAsync("ServerNotification", "Ringtone check ok: " + ringtone.ToString()); }
                else
                {
                    await Clients.Caller.SendAsync("ServerNotification", "Unknown ringtone on checking (will be added): " + ringtone.ToString());
                    ringtone = dataBridge.AddRingtone(ringtone);
                    await Clients.Caller.SendAsync("GetMyRingtone", ringtone);
                }
            }
        }

        [HubMethodName("RemoveRingtone")]
        public async Task RemoveRingtone(Ringtone ringtone)
        {
            if (ringtone.User == Context.User.Identity.Name && dataBridge.RemoveRingtone(ringtone))
            { await Clients.Caller.SendAsync("RemovedRingtone" + ringtone); }
            else
            { await Clients.Caller.SendAsync("ServerNotification", "Error removing alarm."); }
        }

        [HubMethodName("ServerNotification")]
        public async Task ServerNotification(string text)
        {
            Console.WriteLine("From client: " + text);
            await Clients.All.SendAsync("ServerNotification", "Answer: " + text);
        }
    }
}