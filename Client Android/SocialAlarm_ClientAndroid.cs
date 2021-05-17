using Android.App;
using Android.Media;
using Android.Util;
using Android.Widget;
using Client_Android.Activities;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using Xamarin.Essentials;

namespace Client_Android
{
    public class SocialAlarm_ClientAndroid
    {
        private List<Model_Alarm> myAlarms;
        private Adapter_MyAlarms myAlarmsAdapter;
        private List<Model_Alarm> othersAlarms;
        private Adapter_OthersAlarms othersAlarmsAdapter;
        private List<Model_AlarmLog> AlarmsLogs;
        private Adapter_AlarmsLogs AlarmsLogsAdapter;

        private string address = string.Empty;
        Activity activity;

        public HubConnection connection = null;

        public SocialAlarm_ClientAndroid(string address, Activity activity,
         ref List<Model_Alarm> myAlarms, ref List<Model_Alarm> othersAlarms, ref List<Model_AlarmLog> AlarmsLogs,
       ref Adapter_MyAlarms myAlarmsAdapter, ref Adapter_OthersAlarms othersAlarmsAdapter, ref Adapter_AlarmsLogs AlarmsLogsAdapter)
        {
            this.address = address;
            this.myAlarms = myAlarms;
            this.othersAlarms = othersAlarms;
            this.AlarmsLogs = AlarmsLogs;
            this.myAlarmsAdapter = myAlarmsAdapter;
            this.othersAlarmsAdapter = othersAlarmsAdapter;
            this.AlarmsLogsAdapter = AlarmsLogsAdapter;
        }

        public async System.Threading.Tasks.Task ConnectAsync()
        {
            string Credentials = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(Preferences.Get("Login", "") + ":" + Preferences.Get("Password", "")));
            connection = new HubConnectionBuilder().WithUrl(address, options => { options.Headers.Add("Authorization", $"Basic {Credentials}"); }).WithAutomaticReconnect().Build();

            connection.On<string>("ServerNotification", (arg) =>
            {
                switch (arg)
                {
                    case "wrong_alarm_time":
                        Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.wrong_alarm_time), ToastLength.Long).Show();
                        break;
                    case "user_not_online":
                        Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.user_not_online), ToastLength.Long).Show();
                        break;
                }
                Log.Error("HUB: From server: ", arg.ToString());
            });

            connection.On<Model_Alarm, string, string>("GetAlarm", (receivedAlarm, displayedUser, userDecription) =>
            {
                Log.Error("HUB: Получен будильник: ", receivedAlarm.ToString());
                int position;
                if (displayedUser.Trim().Length > 0)
                    receivedAlarm.DisplayedName = displayedUser;
                if (userDecription.Trim().Length > 0)
                    receivedAlarm.UserDecription = userDecription;
                if (receivedAlarm.User == Preferences.Get("Login", ""))
                {
                    position = myAlarms.FindIndex(element => receivedAlarm.ID == element.ID);
                    if (position == -1)
                    {
                        myAlarms.Add(receivedAlarm);
                        myAlarms.Sort((a, b) => DateTimeOffset.Compare(a.Time, b.Time));
                        position = myAlarms.FindIndex(element => receivedAlarm == element);
                        myAlarmsAdapter.NotifyDataSetChanged();
                        // myAlarmsAdapter.NotifyItemInserted(myAlarms.IndexOf(receivedAlarm) + 1);
                    }
                    else
                    {
                        myAlarms[position] = receivedAlarm;
                        myAlarms.Sort((a, b) => DateTimeOffset.Compare(b.Time, a.Time));
                        position = myAlarms.FindIndex(element => receivedAlarm == element);
                        myAlarmsAdapter.NotifyItemChanged(myAlarms.IndexOf(receivedAlarm));
                    }
                }
                else
                {
                    position = othersAlarms.FindIndex(element => receivedAlarm.ID == element.ID);
                    if (position == -1)
                    {
                        othersAlarms.Add(receivedAlarm);
                        othersAlarms.Sort((a, b) => DateTimeOffset.Compare(b.Time, a.Time));
                        othersAlarmsAdapter.NotifyDataSetChanged();
                        //othersAlarmsAdapter.NotifyItemInserted(othersAlarms.IndexOf(receivedAlarm));
                    }
                    else
                    {
                        othersAlarms[position] = receivedAlarm;
                        othersAlarms.Sort((a, b) => DateTimeOffset.Compare(b.Time, a.Time));
                        othersAlarmsAdapter.NotifyItemChanged(othersAlarms.IndexOf(receivedAlarm));
                    }
                }
            });

            connection.On<Model_Alarm>("RemovedAlarm", (removedAlarm) =>
            {
                Log.Error("HUB: Удалён будильник: ", removedAlarm.ToString());
                int position1 = othersAlarms.FindIndex(element => removedAlarm.ID == element.ID);
                int position2 = myAlarms.FindIndex(element => removedAlarm.ID == element.ID);
                if (position1 != -1)
                {
                    othersAlarms.RemoveAt(position1);
                    othersAlarmsAdapter.NotifyItemRemoved(position1);
                }
                else if (position2 != -1)
                {
                    myAlarms.RemoveAt(position2);
                    myAlarmsAdapter.NotifyItemRemoved(position2);
                }
            });

            connection.On<Model_AlarmLog, string, string>("GetAlarmLogs", (receivedAlarmLog, waker, sleeper) =>
              {
                  Log.Error("HUB: Получен лог: ", receivedAlarmLog.ToString());
                  if (waker.Trim().Length > 0)
                      receivedAlarmLog.DisplayedNameWaker = waker;
                  if (sleeper.Trim().Length > 0)
                      receivedAlarmLog.DisplayedNameSlept = sleeper;
                  int position;
                  position = AlarmsLogs.FindIndex(element => receivedAlarmLog.ID == element.ID);
                  if (position == -1)
                  {
                      AlarmsLogs.Add(receivedAlarmLog);
                      AlarmsLogs.Sort((a, b) => DateTimeOffset.Compare(b.DateTime, a.DateTime));
                      AlarmsLogsAdapter.NotifyItemInserted(AlarmsLogs.IndexOf(receivedAlarmLog));
                  }
                  else
                  {
                      AlarmsLogs[position] = receivedAlarmLog;
                      AlarmsLogs.Sort((a, b) => DateTimeOffset.Compare(b.DateTime, a.DateTime));
                      AlarmsLogsAdapter.NotifyItemChanged(AlarmsLogs.IndexOf(receivedAlarmLog));
                  }
              });

            connection.On<Model_Ringtone>("AddedRingtone", (arg) =>
            {
                Log.Error("HUB: Добавлен сигнал: ", arg.ToString());
                if (arg.User == Preferences.Get("Login", ""))
                    ActivityMain.settings.AddRingtone(arg);
            });

            connection.On<Model_Ringtone>("RemovedRingtone", (arg) =>
            {
                if (arg.User == Preferences.Get("Login", ""))
                {
                    Log.Error("HUB: Удалён рингтог: ", arg.ToString());
                    int position = ActivityMain.settings.myRingtones.FindIndex(element => arg.ID == element.ID);
                    if (position != -1)
                    {
                        ActivityMain.settings.RemoveRingtone(ActivityMain.settings.myRingtones[position]);
                        ActivityMain.settings.myRingtones.RemoveAt(position);
                    }
                }
            });

            connection.On<Model_Ringtone>("GetMyRingtone", (arg) =>
            {
                Console.WriteLine("Получен личный сигнал: " + arg.ToString());
                if (arg.User == Preferences.Get("Login", ""))
                    ActivityMain.settings.ReceiveRingtone(arg);
            });

            connection.On<Model_Ringtone, int>("GetRingtones", (ringtone, alarmID) =>
            {
                Console.WriteLine("Получен сигнал: " + ringtone.ToString());
                if (ringtone.User != Preferences.Get("Login", "") && (ActivityRing.currentlyRinging != null && ActivityRing.currentlyRinging.ID == alarmID))
                {
                    int position = myAlarms.FindIndex(element => ringtone.ID == element.ID);
                    if (position == -1)
                    {
                        ActivityRing.othersRingtones.Add(ringtone);
                        ActivityRing.adapter_OthersRingtones.NotifyItemInserted(ActivityRing.othersRingtones.IndexOf(ringtone));
                    }
                    else
                    {
                        ActivityRing.othersRingtones[position] = ringtone;
                        ActivityRing.adapter_OthersRingtones.NotifyItemChanged(ActivityRing.othersRingtones.IndexOf(ringtone));
                    }
                }

            });

            connection.On<int, int, string>("RingAlarm", (ringtoneID, alarmID, ringer) =>
            {
                Log.Error("HUB: ", "Ring attempt... ");
                Model_Alarm myAlarm = myAlarms.Find(element => element.ID == alarmID);
                if (myAlarm != null)
                {
                    ActivityAlarming.ringtone.Stop();
                    int position;
                    Model_Ringtone ringtoneData = ActivityMain.settings.myRingtones.Find(element => element.ID == ringtoneID);
                    if (ringtoneData != null)
                    {
                        Log.Error("HUB: ", "Playing " + ringtoneData.ToString());
                        ActivityAlarming.ringtone = RingtoneManager.GetRingtone(Application.Context, Android.Net.Uri.Parse(ringtoneData.File));
                        ActivityAlarming.ringtone.Play();
                        if (!ActivityAlarming.ringtone.IsPlaying)
                        {
                            Log.Error("HUB: ", "Error playing selected ringtone. Playing default one.");
                            ActivityAlarming.ringtone = RingtoneManager.GetRingtone(Application.Context, RingtoneManager.GetActualDefaultRingtoneUri(Application.Context, RingtoneType.Alarm));
                            ActivityAlarming.ringtone.Play();
                        }
                    }
                }
                else
                {
                    Log.Error("HUB: ", "Playing default ringtone.");
                    ActivityAlarming.ringtone = RingtoneManager.GetRingtone(Application.Context, RingtoneManager.GetActualDefaultRingtoneUri(Application.Context, RingtoneType.Alarm));
                }
            });

            connection.Closed += async (ex) =>
            {
                Console.WriteLine(ex.ToString());
                Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.network_error), ToastLength.Long).Show();
                Log.Error("HUB: ", "Connection closed!");
                Thread.Sleep(5000);
                Log.Error("HUB: ", "Reconecting...");
                CustomConnect();
                GetAll();
            };
            connection.Reconnected += async (ex) => GetAll();
            CustomConnect();
        }

        async System.Threading.Tasks.Task CustomConnect()
        {
            while (connection.State != HubConnectionState.Connected)
                try
                {
                    Log.Error("HUB: ", "Connecting...");
                    await connection.StartAsync();
                }
                catch (Exception ex)
                {
                    activity.RunOnUiThread(Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.network_error), ToastLength.Long).Show);
                    Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.network_error), ToastLength.Long).Show();
                }
        }

        public void AddChangeAlarm(Model_Alarm alarm)
        {
            Log.Error("HUB: adding alarm", alarm.ToString());
            try
            { connection.InvokeAsync("AddAlarm", alarm); }
            catch (Exception ex)
            { Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.network_error), ToastLength.Long).Show(); }
        }

        public void GetAlarms()
        {
            try
            {
                myAlarms.Clear();
                othersAlarms.Clear();
                myAlarmsAdapter.NotifyDataSetChanged();
                othersAlarmsAdapter.NotifyDataSetChanged();
                connection.InvokeAsync("GetAlarms");
            }
            catch (Exception ex)
            { Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.network_error), ToastLength.Long).Show(); }
        }

        public void RemoveAlarm(Model_Alarm alarm)
        {
            try
            { connection.InvokeAsync("RemoveAlarm", alarm); }
            catch (Exception ex)
            { Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.network_error), ToastLength.Long).Show(); }
        }

        public void AddChangeRingtone(Model_Ringtone ringtone)
        {
            try
            { connection.InvokeAsync("AddRingtone", ringtone); }
            catch (Exception ex)
            { Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.network_error), ToastLength.Long).Show(); }
        }

        public void GetMyRingtones()
        {
            try
            { connection.InvokeAsync("GetMyRingtone"); }
            catch (Exception ex)
            { Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.network_error), ToastLength.Long).Show(); }
        }

        public void RemoveRingtone(Model_Ringtone ringtone)
        {
            try
            { connection.InvokeAsync("RemoveRingtone", ringtone); }
            catch (Exception ex)
            { Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.network_error), ToastLength.Long).Show(); }
        }

        public void CheckRingtone(Model_Ringtone ringtone)
        {
            try
            { connection.InvokeAsync("CheckRingtone", ringtone); }
            catch (Exception ex)
            { Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.network_error), ToastLength.Long).Show(); }
        }

        public void GetRingtones(Model_Alarm userToAnnoy)
        {
            try
            { connection.InvokeAsync("GetRingtones", userToAnnoy); }
            catch (Exception ex)
            { Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.network_error), ToastLength.Long).Show(); }
        }

        public void RingAlarm(int alarmID, int ringtoneID)
        {
            try
            { connection.InvokeAsync("RingAlarm", alarmID, ringtoneID); }
            catch (Exception ex)
            { Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.network_error), ToastLength.Long).Show(); }
        }

        public void GetAlarmLogs()
        {
            AlarmsLogs.Clear();
            try
            { connection.InvokeAsync("GetAlarmLogs"); }
            catch (Exception ex)
            { Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.network_error), ToastLength.Long).Show(); }
        }

        public void GetAll()
        {
            ActivityMain.settings.SyncRingtones();
            GetMyRingtones();
            GetAlarmLogs();
            GetAlarms();
        }
    }
}