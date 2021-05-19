using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Google.Android.Material.BottomNavigation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Xamarin.Essentials;

namespace Client_Android
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class ActivityMain : AppCompatActivity, BottomNavigationView.IOnNavigationItemSelectedListener
    {
        private List<Model_Alarm> myAlarms = new List<Model_Alarm>();
        private Adapter_MyAlarms myAlarmsAdapter;
        private List<Model_Alarm> othersAlarms = new List<Model_Alarm>();
        private Adapter_OthersAlarms othersAlarmsAdapter;
        private List<Model_AlarmLog> AlarmsLogs = new List<Model_AlarmLog>();
        private Adapter_AlarmsLogs AlarmsLogsAdapter;

        private BottomNavigationView navigation;
        private SwipeRefreshLayout swipeRefreshLayout;
        private RecyclerView recyclerView;

        public static Settings settings = null;
        public static SocialAlarm_ClientAndroid socialAlarm;
        public static Ringtone ringtone = RingtoneManager.GetRingtone(Application.Context, RingtoneManager.GetActualDefaultRingtoneUri(Application.Context, RingtoneType.Alarm));

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);

            SetContentView(Resource.Layout.activity_main);
            swipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);

            recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            recyclerView.SetLayoutManager(new LinearLayoutManager(this)
            {
                ReverseLayout = true,
                StackFromEnd = true
            });
            navigation = FindViewById<BottomNavigationView>(Resource.Id.navigation);
            navigation.SetOnNavigationItemSelectedListener(this);
            navigation.Menu.FindItem(Resource.Id.navigation_my_alarms).SetChecked(true);
        }

        protected override void OnResume()
        {
            base.OnResume();
            ringtone.Stop();
            if ((socialAlarm == null) || (socialAlarm.connection == null) || (socialAlarm.connection.State != Microsoft.AspNetCore.SignalR.Client.HubConnectionState.Connected))
            { CheckSettings(); }
            socialAlarm.activity = this;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
        {
            if (resultCode == Result.Ok)
            {
                switch (requestCode)
                {
                    case 0:
                        socialAlarm.AddChangeAlarm(JsonConvert.DeserializeObject<Model_Alarm>(intent.GetStringExtra("AlarmToEdit")));
                        recyclerView.ScrollToPosition(myAlarms.Count + 1);
                        break;
                    case 1:
                        int position = intent.GetIntExtra("AlarmPosition", 0);
                        socialAlarm.AddChangeAlarm(JsonConvert.DeserializeObject<Model_Alarm>(intent.GetStringExtra("AlarmToEdit")));
                        myAlarmsAdapter.NotifyItemChanged(position);
                        break;
                }
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private async void CheckSettings()
        {
            swipeRefreshLayout.Refreshing = true;

            myAlarmsAdapter = new Adapter_MyAlarms(ref myAlarms, this);
            othersAlarmsAdapter = new Adapter_OthersAlarms(othersAlarms, this);
            AlarmsLogsAdapter = new Adapter_AlarmsLogs(ref AlarmsLogs);

            socialAlarm = new SocialAlarm_ClientAndroid(Preferences.Get("ServerAddress", ""), ref myAlarms, ref othersAlarms, ref AlarmsLogs, ref myAlarmsAdapter, ref othersAlarmsAdapter, ref AlarmsLogsAdapter);
            settings = new Settings(socialAlarm);

            if (Preferences.Get("ServerAddress", "").Length == 0)
            {
                Toast.MakeText(this, Resources.GetString(Resource.String.NoServer), ToastLength.Long).Show();
                Intent intent = new Intent(this, typeof(ActivitySettings));
                StartActivity(intent);
                return;
            }
            else
            if (Preferences.Get("Login", "").Length == 0 || Preferences.Get("Password", "").Length == 0)
            {
                Toast.MakeText(this, Resources.GetString(Resource.String.NoCrendentials), ToastLength.Long).Show();
                Intent intent = new Intent(this, typeof(ActivitySettings));
                StartActivity(intent);
                return;
            }
            else
            {
                await socialAlarm.ConnectAsync();
                socialAlarm.GetAll();
                swipeRefreshLayout.Refreshing = false;
                navigation.SelectedItemId = Resource.Id.navigation_my_alarms;
            }
        }

        private EventHandler refreshHandler;

        private void SetRecycleViewAlarms(bool IsMyAlarms)
        {
            swipeRefreshLayout.Refresh -= refreshHandler;
            if (IsMyAlarms)
            {
                recyclerView.SetAdapter(myAlarmsAdapter);
                refreshHandler = (sender, args) =>
                {
                    socialAlarm.GetMyAlarms();
                    swipeRefreshLayout.Refreshing = false;
                };
            }
            else
            {
                recyclerView.SetAdapter(othersAlarmsAdapter);
                refreshHandler = (sender, args) =>
                    {
                        socialAlarm.GetAlarms();
                        swipeRefreshLayout.Refreshing = false;
                    };
            }
            swipeRefreshLayout.Refresh += refreshHandler;
        }

        private void SetRecycleViewLogs()
        {
            swipeRefreshLayout.Refresh -= refreshHandler;
            recyclerView.SetAdapter(AlarmsLogsAdapter);
            refreshHandler = (sender, args) =>
            {
                socialAlarm.GetAlarmLogs();
                swipeRefreshLayout.Refreshing = false;
            };
            swipeRefreshLayout.Refresh += refreshHandler;
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.navigation_add_alarm:
                    {
                        Intent intent = new Intent(this, typeof(ActivityAddAlarm));
                        StartActivityForResult(intent, 0);
                        return false;
                    }
                case Resource.Id.navigation_my_alarms:
                    {
                        SetRecycleViewAlarms(true);
                        return true;
                    }
                case Resource.Id.navigation_wake_us:
                    {
                        SetRecycleViewAlarms(false);
                        return true;
                    }
                case Resource.Id.navigation_logs:
                    {
                        SetRecycleViewLogs();
                        return true;
                    }
                case Resource.Id.navigation_settings:
                    {
                        Intent intent = new Intent(this, typeof(ActivitySettings));
                        StartActivity(intent);
                        return false;
                    }
            }
            return false;
        }
    }
}