using DataModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Social_Alarm_Server
{
    [Authorize]
    public class SocialAlarm_Server : Hub
    {
        private DataBridge dataBridge = new();
        private static Dictionary<string, HubCallerContext> usersContexts = new();

        int DelayTime = 100;

        public override async Task OnConnectedAsync()
        {
            var user = Context.User.Identity.Name;
            if (user.Length > 0)
                if (usersContexts.ContainsKey(user))
                {
                    Console.WriteLine(user + " already connected! Closing existing connection...");
                    usersContexts[user].Abort();
                    usersContexts.Remove(user);
                    usersContexts[user] = Context;
                }
                else
                {
                    Console.WriteLine(user + " connected!");
                    //await Clients.All.SendAsync("NewUsersCount", usersContexts.Count);
                    usersContexts.Add(user, Context);
                    Console.Write("All connected users: ");
                    foreach (KeyValuePair<string, HubCallerContext> pair in usersContexts)
                        Console.Write("[" + pair.Key + "]");
                    Console.WriteLine();
                }
            else Context.Abort();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            if (usersContexts.TryGetValue(Context.User.Identity.Name, out HubCallerContext temp))
                if (Context.ConnectionId == temp.ConnectionId)
                    usersContexts.Remove(temp.User.Identity.Name);
            Console.Write(Context.User.Identity.Name + " disconnected! ");
            if (usersContexts.Count > 0)
            {
                Console.Write("List of connected users: ");
                Console.WriteLine(string.Join(", ", usersContexts.Keys));
            }
            else
                Console.WriteLine("No users connected.");
        }

        [HubMethodName("GetAlarms")]
        public async Task SendAlarms()
        {
           var alarms =  dataBridge.GetAlarms();
            foreach (Alarm alarm in alarms)
            {
                Console.WriteLine(alarm.ToString());
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
                await Clients.Caller.SendAsync("ServerNotification", "Error adding alarm.");
        }

        [HubMethodName("RemoveAlarm")]
        public async Task RemoveAlarm(Alarm alarm)
        {
            if (alarm.User == Context.User.Identity.Name)
                if (dataBridge.RemoveAlarm(alarm))
                    await Clients.All.SendAsync("RemovedAlarm", alarm);
                else
                    await Clients.Caller.SendAsync("ServerNotification", "Error removing alarm.");
        }

        [HubMethodName("GetAlarmLogs")]
        public async Task SendAlarmsLogs()
        {
            foreach (AlarmLog alarmLog in dataBridge.GetAlarmLogs(Context.User.Identity.Name))
                if (alarmLog.UserSlept == Context.User.Identity.Name || alarmLog.UserWaker == Context.User.Identity.Name)
                {
                    await Clients.Caller.SendAsync("GetAlarmLogs", alarmLog, dataBridge.GetDisplayedName(alarmLog.UserWaker), dataBridge.GetDisplayedName(alarmLog.UserSlept));
                    Thread.Sleep(DelayTime);
                }
        }

        [HubMethodName("RingAlarm")] // "GetRingtones"
        public async Task PrepareToRingAlarm(int alarmID, int ringtoneID)
        {
            Alarm alarm = dataBridge.GetAlarm(alarmID);
            DateTimeOffset temp = new DateTimeOffset(1, 1, 1, DateTimeOffset.Now.Hour, DateTimeOffset.Now.Minute, DateTimeOffset.Now.Second, DateTimeOffset.Now.Offset).AddDays(1);

            Console.WriteLine(Context.User.Identity.Name + " atempted to ring alarm of user " + alarm.User + " With ringtone ID=" + ringtoneID.ToString() + ". Alarm:\n" + alarm.ToString() +
                "\nTime marks (begin alarm, current time, exact alarm):\n" +
            alarm.Time.AddMinutes(0 - alarm.Threshold).ToString() + " < " + temp.ToString() + " < " + alarm.Time.ToString() + ". Threshold (min.): " + alarm.Threshold.ToString());

            if ((alarm.Time.AddMinutes(0 - alarm.Threshold) <= temp) && (temp <= alarm.Time))
            {
                string ringer = dataBridge.GetDisplayedName(Context.User.Identity.Name);
                if (ringer == null)
                    ringer = Context.User.Identity.Name;
                if (usersContexts.ContainsKey(alarm.User))
                    await Clients.User(usersContexts[alarm.User].UserIdentifier).SendAsync("RingAlarm", ringtoneID, alarmID, ringer);
                else
                    await Clients.Caller.SendAsync("ServerNotification", "user_not_online");
            }
            else
                await Clients.Caller.SendAsync("ServerNotification", "wrong_alarm_time");
        }

        [HubMethodName("IamAwake")] //Generates AlarmLog
        public async Task CreateAlarmLog(Alarm alarm, DateTime WokeUpTime, string Waker)
        {
            AlarmLog newLog = null;
            if (alarm.User == Context.User.Identity.Name)
                newLog = dataBridge.AddAlarmLog(alarm, WokeUpTime, Waker);
            if (newLog != null)
            {
                await Clients.Users(new List<string>() { newLog.UserWaker, newLog.UserSlept })
                    .SendAsync("GetAlarmLogs", newLog, dataBridge.GetDisplayedName(newLog.UserWaker), dataBridge.GetDisplayedName(newLog.UserSlept));
            }
        }
        [HubMethodName("GetRingtones")]
        public async Task SendRingtones(Alarm userToAnnoy)
        {
            {
                Ringtone defaultRingtone = new Ringtone();
                defaultRingtone.ID = 0;
                defaultRingtone.User = userToAnnoy.User;
                defaultRingtone.RingtoneName = "¯\\_(ツ)_/¯ ";
                defaultRingtone.Description = "¯\\_(ツ)_/¯ ";
                await Clients.Caller.SendAsync("GetRingtones", defaultRingtone, userToAnnoy.ID);
                Thread.Sleep(DelayTime);
            }
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
                await Clients.Caller.SendAsync("ServerNotification", "Error adding ringtone.");
        }

        [HubMethodName("CheckRingtone")]
        public async Task CheckRingtone(Ringtone ringtone)
        {
            if (ringtone.User == Context.User.Identity.Name)
            {
                if (dataBridge.CheckRingtone(ringtone))
                    await Clients.Caller.SendAsync("ServerNotification", "Ringtone check ok: " + ringtone.ToString());
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
            if (ringtone.User == Context.User.Identity.Name)
            {
                if (dataBridge.RemoveRingtone(ringtone))
                    await Clients.Caller.SendAsync("RemovedRingtone" + ringtone);
                else
                    await Clients.Caller.SendAsync("ServerNotification", "Error removing alarm.");
            }
        }

        [HubMethodName("ServerNotification")]
        public async Task ServerNotification(string arg)
        {
            Console.WriteLine("From client:" + arg);
            await Clients.All.SendAsync("ServerNotification", "Answer:" + arg);
        }
    }
}