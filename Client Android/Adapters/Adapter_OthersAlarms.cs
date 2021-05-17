using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using System;
using System.Collections.Generic;

namespace Client_Android
{
    public class Adapter_OthersAlarms : RecyclerView.Adapter
    {
        private List<Model_Alarm> othersAlarms;
        private ViewHolder_OthersAlarmsAdapter viewHolder;
        private Activity activity;

        public Adapter_OthersAlarms(List<Model_Alarm> OthersAlarms, Activity activity)
        {
            othersAlarms = OthersAlarms;
            this.activity = activity;
        }

        public override int ItemCount => othersAlarms.Count;

        /*
         * Custom binding element in: List<AlarmModel> OthersAlarms
         * to RecycleView (on activity).         
         */
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            this.viewHolder = (ViewHolder_OthersAlarmsAdapter)viewHolder;
            this.viewHolder.adapter = this;

            if (othersAlarms[position].IsWaker)
                this.viewHolder.textViewIsAlarm.Text = Application.Context.Resources.GetString(Resource.String.isAlarm);
            else
                this.viewHolder.textViewIsAlarm.Text = Application.Context.Resources.GetString(Resource.String.isReminder);
            this.viewHolder.textViewTime.Text = othersAlarms[position].Time.ToLocalTime().Hour.ToString().PadLeft(2, '0') + ":" + othersAlarms[position].Time.ToLocalTime().Minute.ToString().PadLeft(2, '0');
            DateTimeOffset tempTime = othersAlarms[position].Time.AddDays(1).AddMinutes(othersAlarms[position].Threshold);
            this.viewHolder.textViewThreshold.Text = tempTime.ToLocalTime().Hour.ToString().PadLeft(2, '0') + ":" + tempTime.ToLocalTime().Minute.ToString().PadLeft(2, '0');
            if (othersAlarms[position].Description != null && othersAlarms[position].Description.Trim().Length > 0)
                this.viewHolder.textViewDescription.Text = othersAlarms[position].Description;
            if (othersAlarms[position].DisplayedName != null && othersAlarms[position].DisplayedName.Trim().Length > 0)
                this.viewHolder.textViewUsername.Text = othersAlarms[position].DisplayedName.Trim();
            else
                this.viewHolder.textViewUsername.Text = othersAlarms[position].User;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.element_othersAlarm, parent, false);
            return new ViewHolder_OthersAlarmsAdapter(itemView, activity);
        }

        [Serializable]
        public class ViewHolder_OthersAlarmsAdapter : RecyclerView.ViewHolder
        {
            public TextView textViewIsAlarm { get; set; }
            public TextView textViewTime { get; set; }
            public TextView textViewThreshold { get; set; }
            public TextView textViewUsername { get; set; }
            public TextView textViewDescription { get; set; }
            public Button buttonRing { get; set; }
            public Adapter_OthersAlarms adapter { get; set; }

            private Activity activity;

            public ViewHolder_OthersAlarmsAdapter(View itemView, Activity activity) : base(itemView)
            {
                this.activity = activity;
                textViewIsAlarm = itemView.FindViewById<TextView>(Resource.Id.textViewIsAlarm);
                textViewTime = itemView.FindViewById<TextView>(Resource.Id.textViewTime);
                textViewThreshold = itemView.FindViewById<TextView>(Resource.Id.textViewThreshold);
                textViewUsername = itemView.FindViewById<TextView>(Resource.Id.textViewUsername);
                textViewDescription = itemView.FindViewById<TextView>(Resource.Id.textViewDescription);
                buttonRing = itemView.FindViewById<Button>(Resource.Id.buttonRing);

                buttonRing.Click += (e, o) => RingItem(AdapterPosition);
            }
            /*
             * Delete button action:
             * deletes alarm localy and from server.
             */
            public void RingItem(int position)
            {
                Intent intent = new Intent(activity, typeof(ActivityRing));
                ActivityRing.othersRingtones = new List<Model_Ringtone>();
                try
                {
                    ActivityRing.currentlyRinging = adapter.othersAlarms[AdapterPosition];
                    ActivityMain.socialAlarm.GetRingtones(ActivityRing.currentlyRinging);
                    activity.StartActivityForResult(intent, 0);
                }
                catch { }
            }
        }
    }
}