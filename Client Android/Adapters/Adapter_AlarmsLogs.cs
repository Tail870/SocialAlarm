using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using System;
using System.Collections.Generic;

namespace Client_Android
{
    public class Adapter_AlarmsLogs : RecyclerView.Adapter
    {
        private List<Model_AlarmLog> alarmsLogs;
        private ViewHolder_AlarmsLogsAdapter viewHolder;

        public Adapter_AlarmsLogs(ref List<Model_AlarmLog> alarmsLogs) => this.alarmsLogs = alarmsLogs;

        public override int ItemCount => alarmsLogs.Count;

        /*
         * Custom binding element in: List<AlarmLogModel> AlarmsLogs
         * to RecycleView (on activity).         
         */
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            this.viewHolder = (ViewHolder_AlarmsLogsAdapter)viewHolder;
            this.viewHolder.adapter = this;

            if (alarmsLogs[position].IsWaker)
                this.viewHolder.textViewIsAlarm.Text = Application.Context.Resources.GetString(Resource.String.isAlarm);
            else
                this.viewHolder.textViewIsAlarm.Text = Application.Context.Resources.GetString(Resource.String.isReminder);
            this.viewHolder.textViewTime.Text = alarmsLogs[position].DateTime.ToLocalTime().ToString();

            if (alarmsLogs[position].Description != null && alarmsLogs[position].Description.Trim().Length > 0)
                this.viewHolder.textViewDescription.Text = alarmsLogs[position].Description;

            if (alarmsLogs[position].DisplayedNameSlept != null && alarmsLogs[position].DisplayedNameSlept.Trim().Length > 0)
                this.viewHolder.textViewUsername.Text = alarmsLogs[position].DisplayedNameSlept;
            else
                this.viewHolder.textViewUsername.Text = alarmsLogs[position].UserSlept;

            if (alarmsLogs[position].DisplayedNameSlept != null && alarmsLogs[position].DisplayedNameSlept.Trim().Length > 0)
                this.viewHolder.textViewUsernameWaker.Text = alarmsLogs[position].DisplayedNameWaker;
            else
                this.viewHolder.textViewUsernameWaker.Text = alarmsLogs[position].UserWaker;
        }


        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.element_alarmLog, parent, false);
            return new ViewHolder_AlarmsLogsAdapter(itemView);
        }

        [Serializable]
        public class ViewHolder_AlarmsLogsAdapter : RecyclerView.ViewHolder
        {
            public TextView textViewIsAlarm { get; set; }
            public TextView textViewTime { get; set; }
            public TextView textViewThreshold { get; set; }
            public TextView textViewUsername { get; set; }
            public TextView textViewUsernameWaker { get; set; }
            public TextView textViewDescription { get; set; }
            public Button buttonRing { get; set; }
            public Adapter_AlarmsLogs adapter { get; set; }

            public ViewHolder_AlarmsLogsAdapter(View itemView) : base(itemView)
            {
                textViewIsAlarm = itemView.FindViewById<TextView>(Resource.Id.textViewIsAlarm);
                textViewTime = itemView.FindViewById<TextView>(Resource.Id.textViewTime);
                textViewUsername = itemView.FindViewById<TextView>(Resource.Id.textViewUsername);
                textViewUsernameWaker = itemView.FindViewById<TextView>(Resource.Id.textViewUsernameWaker);
                textViewDescription = itemView.FindViewById<TextView>(Resource.Id.textViewDescription);
            }
        }
    }
}