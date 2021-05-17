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

        private Model_Alarm alarm;

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
                alarm = JsonConvert.DeserializeObject<Model_Alarm>(alarmJSON);
                toggleButtonIsWaker.Checked = alarm.IsWaker;
                editTextDescription.Text = alarm.Description;
                DateTimeOffset time = alarm.Time.AddDays(1);
                timePicker.CurrentHour = (Java.Lang.Integer)time.Hour;
                timePicker.CurrentMinute = (Java.Lang.Integer)time.Minute;
                time = time.AddMinutes(-alarm.Threshold);
                timePickerThreshold.CurrentHour = (Java.Lang.Integer)alarm.Time.Hour;
                timePickerThreshold.CurrentMinute = (Java.Lang.Integer)alarm.Time.Minute;
            }
        }

        private void SaveChanges()
        {
            if (alarm == null)
                alarm = new Model_Alarm();
            alarm.User = Preferences.Get("Login", "");
            alarm.IsWaker = toggleButtonIsWaker.Checked;
            alarm.Description = editTextDescription.Text;

            int timeStart = ((int)timePickerThreshold.CurrentHour * 60) + (int)timePickerThreshold.CurrentMinute;
            int timeEnd = ((int)timePicker.CurrentHour * 60) + (int)timePicker.CurrentMinute;
            if (timeStart <= timeEnd)
                alarm.Threshold = timeEnd - timeStart;
            else
                alarm.Threshold = timeStart - timeEnd;
            Log.Error("----", timeStart.ToString() + "    " + timeEnd.ToString());
            alarm.Time = new DateTimeOffset(2,2, 2, (int)timePicker.CurrentHour, (int)timePicker.CurrentMinute, 0, DateTimeOffset.Now.Offset);
            this.Intent.PutExtra("AlarmToEdit", JsonConvert.SerializeObject(alarm));
            SetResult(Result.Ok, this.Intent);
            Finish();
        }
    }
}