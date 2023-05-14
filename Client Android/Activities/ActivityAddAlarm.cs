using Android.App;
using Android.Content;
using Android.OS;
using Android.Text.Format;
using Android.Util;
using Android.Widget;
using AndroidX.AppCompat.App;
using Newtonsoft.Json;
using System;
using Xamarin.Essentials;

namespace Client_Android
{

    [Activity(Label = "@string/title_alarm_editor", Theme = "@style/AppTheme")]
    public class ActivityAddAlarm : AppCompatActivity
    {
        public Button buttonSave { get; set; }
        public Button buttonCancel { get; set; }
        public TextView textViewUsername { get; set; }
        public ToggleButton toggleButtonIsWaker { get; set; }
        public EditText editTextDescription { get; set; }
        public TimePicker timePicker { get; set; }
        public TimePicker timePickerThreshold { get; set; }

        private Alarm alarm;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_addAlarm);
            textViewUsername = FindViewById<TextView>(Resource.Id.textViewUsername);
            textViewUsername.Text = Preferences.Get("Login", "");
            toggleButtonIsWaker = FindViewById<ToggleButton>(Resource.Id.toggleButtonIsWaker);
            editTextDescription = FindViewById<EditText>(Resource.Id.editTextDescription);
            timePicker = FindViewById<TimePicker>(Resource.Id.timePicker);
            timePickerThreshold = FindViewById<TimePicker>(Resource.Id.timePickerThreshold);
            if (DateFormat.Is24HourFormat(this))
            {
                timePicker.SetIs24HourView((Java.Lang.Boolean)true);
                timePickerThreshold.SetIs24HourView((Java.Lang.Boolean)true);
            }
            buttonSave = FindViewById<Button>(Resource.Id.buttonSave);
            buttonCancel = FindViewById<Button>(Resource.Id.buttonCancel);
            buttonSave.Click += (e, o) => SaveChanges();
            buttonCancel.Click += (e, o) => Finish();

            string alarmJSON = Intent.GetStringExtra("AlarmToEdit");
            if (alarmJSON != null)
            {
                alarm = JsonConvert.DeserializeObject<Alarm>(alarmJSON);
                toggleButtonIsWaker.Checked = alarm.IsWaker;
                editTextDescription.Text = alarm.Description;
                DateTimeOffset time = alarm.Time.ToLocalTime();
                timePicker.Hour = time.Hour;
                timePicker.Minute = time.Minute;
                time = time.AddMinutes(-alarm.Threshold);
                timePickerThreshold.Hour = time.Hour;
                timePickerThreshold.Minute = time.Minute;
            }
        }

        private void SaveChanges()
        {
            if (alarm == null)
            { alarm = new Alarm(); }
            alarm.User = Preferences.Get("Login", "");
            alarm.IsWaker = toggleButtonIsWaker.Checked;
            alarm.Description = editTextDescription.Text;

            int timeStart = (timePickerThreshold.Hour * 60) + timePickerThreshold.Minute;
            int timeEnd = (timePicker.Hour * 60) + timePicker.Minute;
            // Check if starting alarm time less than it's ending
            if (timeStart < timeEnd)
            { alarm.Threshold = timeEnd - timeStart; }
            // If not - correct the threshold (min.) variable
            else
            { alarm.Threshold = 1440 - (timeStart - timeEnd); }
            alarm.Time = new DateTimeOffset(2, 2, 2, timePicker.Hour, timePicker.Minute, 0, DateTimeOffset.Now.Offset);
            Log.Debug("HUB: New|Edited alarm: ", alarm.ToString());
            Intent.PutExtra("AlarmToEdit", JsonConvert.SerializeObject(alarm));
            SetResult(Result.Ok, Intent);
            Finish();
        }
    }
}