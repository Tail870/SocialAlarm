using Android.App;
using Android.Media;
using Android.Util;
using Android.Widget;
using Microsoft.AspNetCore.SignalR.Client;
using Plugin.LocalNotification;
using System;
using System.Collections.Generic;
using System.Threading;
using Xamarin.Essentials;
using Xamarin.Forms;
using Application = Android.App.Application;

namespace Client_Android
{
    public class SocialAlarm_ClientAndroid : ContentPage
    {
        private readonly List<Model_Alarm> myAlarms;
        private readonly Adapter_MyAlarms myAlarmsAdapter;
        private readonly List<Model_Alarm> othersAlarms;
        private readonly Adapter_OthersAlarms othersAlarmsAdapter;
        private readonly List<Model_AlarmLog> AlarmsLogs;
        private readonly Adapter_AlarmsLogs AlarmsLogsAdapter;
        private int ringNotificationCounter = 0;
        private readonly string address = string.Empty;
        public Activity activity;
        private AlertDialog dialog;

        public HubConnection connection = null;

        public SocialAlarm_ClientAndroid(string address, ref List<Model_Alarm> myAlarms, ref List<Model_Alarm> othersAlarms, ref List<Model_AlarmLog> AlarmsLogs, ref Adapter_MyAlarms myAlarmsAdapter, ref Adapter_OthersAlarms othersAlarmsAdapter, ref Adapter_AlarmsLogs AlarmsLogsAdapter)
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
                Log.Debug("HUB: From server: ", arg.ToString());
            });

            connection.On<Model_Alarm, string, string>("GetAlarm", (receivedAlarm, displayedUser, userDecription) =>
            {
                Log.Debug("HUB: Получен будильник: ", receivedAlarm.ToString());
                int position;
                if (displayedUser.Trim().Length > 0)
                { receivedAlarm.DisplayedName = displayedUser; }
                if (userDecription.Trim().Length > 0)
                { receivedAlarm.UserDecription = userDecription; }
                if (receivedAlarm.User == Preferences.Get("Login", ""))
                {
                    position = myAlarms.FindIndex(element => receivedAlarm.ID == element.ID);
                    if (position == -1)
                    {
                        myAlarms.Add(receivedAlarm);
                        myAlarms.Sort((a, b) => DateTimeOffset.Compare(a.Time.ToLocalTime(), b.Time.ToLocalTime()));
                        position = myAlarms.FindIndex(element => receivedAlarm == element);
                        myAlarmsAdapter.NotifyItemInserted(myAlarms.IndexOf(receivedAlarm) + 1);
                    }
                    else
                    {
                        myAlarms[position] = receivedAlarm;
                        myAlarms.Sort((a, b) => DateTimeOffset.Compare(b.Time.ToLocalTime(), a.Time.ToLocalTime()));
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
                        othersAlarms.Sort((a, b) => DateTimeOffset.Compare(b.Time.ToLocalTime(), a.Time.ToLocalTime()));
                        othersAlarmsAdapter.NotifyItemInserted(othersAlarms.IndexOf(receivedAlarm));
                    }
                    else
                    {
                        othersAlarms[position] = receivedAlarm;
                        othersAlarms.Sort((a, b) => DateTimeOffset.Compare(b.Time.ToLocalTime(), a.Time.ToLocalTime()));
                        othersAlarmsAdapter.NotifyItemChanged(othersAlarms.IndexOf(receivedAlarm));
                    }
                }
            });

            connection.On<Model_Alarm>("RemovedAlarm", (removedAlarm) =>
            {
                Log.Debug("HUB: Удалён будильник: ", removedAlarm.ToString());
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
                  Log.Debug("HUB: Получен лог: ", receivedAlarmLog.ToString());
                  if (waker.Trim().Length > 0)
                  { receivedAlarmLog.DisplayedNameWaker = waker; }
                  if (sleeper.Trim().Length > 0)
                  { receivedAlarmLog.DisplayedNameSlept = sleeper; }
                  int position;
                  position = AlarmsLogs.FindIndex(element => receivedAlarmLog.ID == element.ID);
                  if (position == -1)
                  {
                      AlarmsLogs.Add(receivedAlarmLog);
                      AlarmsLogs[position] = receivedAlarmLog;
                      AlarmsLogs.Sort((a, b) => DateTimeOffset.Compare(b.DateTime.ToLocalTime(), a.DateTime.ToLocalTime()));
                      AlarmsLogsAdapter.NotifyItemInserted(AlarmsLogs.IndexOf(receivedAlarmLog));
                  }
                  else
                  {
                      AlarmsLogs[position] = receivedAlarmLog;
                      AlarmsLogs.Sort((a, b) => DateTimeOffset.Compare(b.DateTime.ToLocalTime(), a.DateTime.ToLocalTime()));
                      AlarmsLogsAdapter.NotifyItemChanged(AlarmsLogs.IndexOf(receivedAlarmLog));
                  }
              });

            connection.On<Model_Ringtone>("AddedRingtone", (myRingtone) =>
            {
                Log.Debug("HUB: Добавлен сигнал: ", myRingtone.ToString());
                if (myRingtone.User == Preferences.Get("Login", ""))
                { ActivityMain.settings.AddRingtone(myRingtone); }
            });

            connection.On<Model_Ringtone>("RemovedRingtone", (arg) =>
            {
                if (arg.User == Preferences.Get("Login", ""))
                {
                    Log.Debug("HUB: Удалён рингтог: ", arg.ToString());
                    int position = ActivityMain.settings.myRingtones.FindIndex(element => arg.ID == element.ID);
                    if (position != -1)
                    {
                        ActivityMain.settings.RemoveRingtone(ActivityMain.settings.myRingtones[position]);
                        ActivityMain.settings.myRingtones.RemoveAt(position);
                    }
                }
            });

            connection.On<Model_Ringtone>("GetMyRingtone", (myRingtone) =>
            {
                Console.WriteLine("Получен личный сигнал: " + myRingtone.ToString());
                if (myRingtone.User == Preferences.Get("Login", ""))
                { ActivityMain.settings.ReceiveRingtone(myRingtone); }
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

            connection.On<int>("StopRinging", (alarmID) =>
            {
                activity.Finish();
                Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.user_woke_up), ToastLength.Long).Show();
            });

            connection.On<int, string, string>("FinishAlarm", async (alarmID, ringer, ringerDisplay) =>
            {
                Log.Debug("HUB: ", "Finish alarm atempt... ");
                Model_Alarm myAlarm = myAlarms.Find(element => element.ID == alarmID);
                if (myAlarm != null)
                {
                    DateTimeOffset tempTime = myAlarm.Time.AddDays(1).ToLocalTime();
                    DateTimeOffset currentTime = new DateTimeOffset(1, 1, 2, DateTimeOffset.Now.Hour, DateTimeOffset.Now.Minute, DateTimeOffset.Now.Second, DateTimeOffset.Now.Offset);
                    if ((myAlarm.Time.AddMinutes(0 - myAlarm.Threshold) <= currentTime) && (currentTime <= tempTime))
                    {
                        if (dialog != null)
                        {
                            dialog.Dismiss();
                        }

                        dialog = new AlertDialog.Builder(activity)
                            .SetNegativeButton(Application.Context.Resources.GetString(Resource.String.no), (c, ev) => { })
                            .SetPositiveButton(Application.Context.Resources.GetString(Resource.String.yes), async (c, ev) => await FinishAlarmAsync(true, alarmID, ringer))
                            .SetIcon(Resource.Drawable.ic_clock_black_24dp)
                            .SetTitle(Application.Context.Resources.GetString(Resource.String.alarm_finish_atempt))
                            .SetMessage(Application.Context.Resources.GetString(Resource.String.alarm_finish_atempt_yn))
                            .Create();
                        dialog.Show();
                    }
                }
            });

            connection.On<int, int, string>("RingAlarm", (ringtoneID, alarmID, ringer) =>
            {
                Log.Debug("HUB: ", "Ring attempt... ");
                Model_Alarm myAlarm = myAlarms.Find(element => element.ID == alarmID);
                ActivityMain.ringtone.Stop();
                if (myAlarm != null)
                {
                    DateTimeOffset tempTime = myAlarm.Time.AddDays(1).ToLocalTime();
                    DateTimeOffset currentTime = new DateTimeOffset(1, 1, 2, DateTimeOffset.Now.Hour, DateTimeOffset.Now.Minute, DateTimeOffset.Now.Second, DateTimeOffset.Now.Offset);
                    if ((myAlarm.Time.AddMinutes(0 - myAlarm.Threshold) <= currentTime) && (currentTime <= tempTime))
                    {
                        Model_Ringtone ringtoneData = ActivityMain.settings.myRingtones.Find(element => element.ID == ringtoneID);
                        if (ringtoneData != null)
                        {
                            Log.Debug("HUB: ", "Playing " + ringtoneData.ToString());
                            ActivityMain.ringtone = RingtoneManager.GetRingtone(Application.Context, Android.Net.Uri.Parse(ringtoneData.File));
                            ActivityMain.ringtone.Play();
                        }
                        else
                        {
                            Log.Debug("HUB: ", "No ringtone found. Playing default ringtone.");
                            ActivityMain.ringtone = RingtoneManager.GetRingtone(Application.Context, RingtoneManager.GetActualDefaultRingtoneUri(Application.Context, RingtoneType.Ringtone));
                        }
                        if (!ActivityMain.ringtone.IsPlaying)
                        {
                            Log.Error("HUB: ", "Error playing selected ringtone. Playing default one.");
                            ActivityMain.ringtone = RingtoneManager.GetRingtone(Application.Context, RingtoneManager.GetActualDefaultRingtoneUri(Application.Context, RingtoneType.Ringtone));
                            ActivityMain.ringtone.Play();
                        }
                        NotificationCenter.Current.Show((notification) => notification
                                        .WithTitle(ringer + " " + Application.Context.Resources.GetString(Resource.String.rang))
                                        .WithDescription(Application.Context.Resources.GetString(Resource.String.alarm_description) + "\n" + myAlarm.Description)
                                        .WithBadgeCount(ringNotificationCounter)
                                         .WithNotificationId(ringNotificationCounter)
                                        .WithAndroidOptions((android) => android
                                            .WithChannelId("social_alarm")
                                            .WithIconName("ic_my_alarms_black_24dp")
                                        .Build())
                                        .Create());
                        ringNotificationCounter++;
                    }
                }
            });

            connection.Closed += async (ex) =>
            {
                Console.WriteLine(ex.ToString());
                Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.network_error), ToastLength.Long).Show();
                Log.Error("HUB: ", "Connection closed!");
                Thread.Sleep(5000);
                Log.Error("HUB: ", "Reconecting...");
                await CustomConnect();
            };

            //  connection.Reconnected += async (ex) => GetAll();

            await CustomConnect();
        }

        private async System.Threading.Tasks.Task FinishAlarmAsync(bool wokeUp, int alarmID, string ringer)
        {
            if (wokeUp)
            {
                try
                { await connection.InvokeAsync("IamAwake", alarmID, ringer); }
                catch (Exception)
                { Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.network_error), ToastLength.Long).Show(); }
                ActivityMain.ringtone.Stop();
            }
        }


        private async System.Threading.Tasks.Task CustomConnect()
        {
            while (connection.State != HubConnectionState.Connected)
            {
                try
                {
                    Log.Debug("HUB: ", "Connecting...");
                    await connection.StartAsync();
                }
                catch (Exception)
                {
                    activity.RunOnUiThread(Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.network_error), ToastLength.Long).Show);
                    Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.network_error), ToastLength.Long).Show();
                }
            }
        }

        public void AddChangeAlarm(Model_Alarm alarm)
        {
            Log.Debug("HUB: adding alarm", alarm.ToString());
            try
            { connection.InvokeAsync("AddAlarm", alarm); }
            catch (Exception)
            { Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.network_error), ToastLength.Long).Show(); }
        }

        public void GetAlarms()
        {
            try
            {
                othersAlarms.Clear();
                myAlarmsAdapter.NotifyDataSetChanged();
                othersAlarmsAdapter.NotifyDataSetChanged();
                connection.InvokeAsync("GetAlarms", false);
            }
            catch (Exception)
            { Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.network_error), ToastLength.Long).Show(); }
        }

        public void GetMyAlarms()
        {
            try
            {
                myAlarms.Clear();
                myAlarmsAdapter.NotifyDataSetChanged();
                othersAlarmsAdapter.NotifyDataSetChanged();
                connection.InvokeAsync("GetAlarms", true);
            }
            catch (Exception)
            { Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.network_error), ToastLength.Long).Show(); }
        }

        public void RemoveAlarm(Model_Alarm alarm)
        {
            try
            { connection.InvokeAsync("RemoveAlarm", alarm); }
            catch (Exception)
            { Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.network_error), ToastLength.Long).Show(); }
        }

        public void AddChangeRingtone(Model_Ringtone ringtone)
        {
            try
            { connection.InvokeAsync("AddRingtone", ringtone); }
            catch (Exception)
            { Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.network_error), ToastLength.Long).Show(); }
        }

        public void GetMyRingtones()
        {
            try
            { connection.InvokeAsync("GetMyRingtone"); }
            catch (Exception)
            { Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.network_error), ToastLength.Long).Show(); }
        }

        public void RemoveRingtone(Model_Ringtone ringtone)
        {
            try
            { connection.InvokeAsync("RemoveRingtone", ringtone); }
            catch (Exception)
            { Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.network_error), ToastLength.Long).Show(); }
        }

        public void CheckRingtone(Model_Ringtone ringtone)
        {
            try
            { connection.InvokeAsync("CheckRingtone", ringtone); }
            catch (Exception)
            { Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.network_error), ToastLength.Long).Show(); }
        }

        public void GetRingtones(Model_Alarm userToAnnoy)
        {
            try
            { connection.InvokeAsync("GetRingtones", userToAnnoy); }
            catch (Exception)
            { Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.network_error), ToastLength.Long).Show(); }
        }

        public void RingAlarm(int alarmID, int ringtoneID)
        {
            try
            { connection.InvokeAsync("RingAlarm", alarmID, ringtoneID); }
            catch (Exception)
            { Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.network_error), ToastLength.Long).Show(); }
        }

        public void FinishAlarm(int alarmID)
        {
            try
            {
                connection.InvokeAsync("FinishAlarm", alarmID);
                Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.alarm_finish), ToastLength.Long).Show();
            }
            catch (Exception)
            { Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.network_error), ToastLength.Long).Show(); }
        }

        public void GetAlarmLogs()
        {
            AlarmsLogs.Clear();
            try
            { connection.InvokeAsync("GetAlarmLogs"); }
            catch (Exception)
            { Toast.MakeText(Application.Context, Application.Context.Resources.GetString(Resource.String.network_error), ToastLength.Long).Show(); }
        }

        public void GetAll()
        {
            ActivityMain.settings.SyncRingtones();
            GetMyRingtones();
            GetAlarmLogs();
            GetMyAlarms();
            GetAlarms();
        }
    }
}