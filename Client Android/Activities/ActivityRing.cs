using Android.App;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.RecyclerView.Widget;
using System;
using System.Collections.Generic;

namespace Client_Android
{
    [Activity(Theme = "@style/AppTheme")]
    public class ActivityRing : AppCompatActivity
    {
        TextView textViewUsername { get; set; }
        TextView textViewUserDescription { get; set; }
        TextView textViewTime { get; set; }
        TextView textViewThreshold { get; set; }
        TextView textViewAlarmDescription { get; set; }
        RecyclerView recyclerView;

        Button buttonCancel { get; set; }
        static Button buttonFinish { get; set; }

        public static List<Model_Ringtone> othersRingtones;
        public static Model_Alarm currentlyRinging;
        public static Adapter_OthersRingtones adapter_OthersRingtones;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            SetContentView(Resource.Layout.activity_ring);

            recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            RecyclerView.LayoutManager recyclerViewLinearLayoutManager = new LinearLayoutManager(this);
            ((LinearLayoutManager)recyclerViewLinearLayoutManager).ReverseLayout = true;
            ((LinearLayoutManager)recyclerViewLinearLayoutManager).StackFromEnd = true;
            recyclerView.SetLayoutManager(recyclerViewLinearLayoutManager);

            string temp;

            temp = currentlyRinging.DisplayedName;
            textViewUsername = FindViewById<TextView>(Resource.Id.textViewUsername);
            if (temp != null && temp.Trim().Length > 0)
                textViewUsername.Text = currentlyRinging.DisplayedName;
            else
                textViewUsername.Text = currentlyRinging.User;
            textViewUserDescription = FindViewById<TextView>(Resource.Id.textViewUserDescription);
            temp = currentlyRinging.UserDecription;
            if (temp != null && temp.Trim().Length > 0)
                textViewUserDescription.Text = currentlyRinging.UserDecription;
            textViewTime = FindViewById<TextView>(Resource.Id.textViewTime);
            textViewTime.Text = currentlyRinging.Time.ToLocalTime().Hour.ToString().PadLeft(2, '0') + ":" + currentlyRinging.Time.ToLocalTime().Minute.ToString().PadLeft(2, '0');
            if (currentlyRinging.IsWaker)
                Title = Application.Context.Resources.GetString(Resource.String.isAlarm);
            else
                Title = Application.Context.Resources.GetString(Resource.String.isReminder);
            textViewThreshold = FindViewById<TextView>(Resource.Id.textViewThreshold);
            DateTimeOffset tempTime = currentlyRinging.Time.AddDays(1).AddMinutes(currentlyRinging.Threshold);
            textViewThreshold.Text = tempTime.ToLocalTime().Hour.ToString().PadLeft(2, '0') + ":" + tempTime.ToLocalTime().Minute.ToString().PadLeft(2, '0');
            textViewAlarmDescription = FindViewById<TextView>(Resource.Id.textViewAlarmDescription);
            temp = currentlyRinging.Description;
            if (temp != null && temp.Trim().Length > 0)
                textViewAlarmDescription.Text = currentlyRinging.Description;

            adapter_OthersRingtones = new Adapter_OthersRingtones(othersRingtones, currentlyRinging);
            recyclerView.SetAdapter(adapter_OthersRingtones);
            adapter_OthersRingtones.NotifyDataSetChanged();

            buttonFinish = FindViewById<Button>(Resource.Id.buttonFinish);
            buttonFinish.Click += (e, o) => CleanUp();
            buttonCancel = FindViewById<Button>(Resource.Id.buttonCancel);
            buttonCancel.Click += (e, o) => CancelRinging();
        }

        private void CancelRinging() => CleanUp();

        //TODO FinishRinging
        private void FinishRinging()
        {

            //CleanUp()
        }

        private void CleanUp()
        {
            othersRingtones = null;
            currentlyRinging = null;
            adapter_OthersRingtones = null;
            Finish();
        }
    }
}