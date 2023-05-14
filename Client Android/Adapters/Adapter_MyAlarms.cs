using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Client_Android
{
    public class Adapter_MyAlarms : RecyclerView.Adapter
    {
        private List<Alarm> myAlarms;
        private ViewHolder_MyAlarmAdapter viewHolder;
        public Activity activity;

        public Adapter_MyAlarms(ref List<Alarm> MyAlarms, Activity activity)
        {
            myAlarms = MyAlarms;
            this.activity = activity;
        }

        public override int ItemCount => myAlarms.Count;

        /*
         * Custom binding element in: List<AlarmModel> myAlarms
         * to RecycleView (on activity).         
         */
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            this.viewHolder = (ViewHolder_MyAlarmAdapter)viewHolder;
            this.viewHolder.adapter = this;
            this.viewHolder.SetMyAlarms(ref myAlarms);
            if (myAlarms[position].IsWaker)
            { this.viewHolder.textViewIsAlarm.Text = Application.Context.Resources.GetString(Resource.String.isAlarm); }
            else
            { this.viewHolder.textViewIsAlarm.Text = Application.Context.Resources.GetString(Resource.String.isReminder); }
            DateTimeOffset tempTime = myAlarms[position].Time.AddDays(1).ToLocalTime();
            this.viewHolder.textViewTime.Text = tempTime.Hour.ToString().PadLeft(2, '0') + ":" + tempTime.Minute.ToString().PadLeft(2, '0');
            tempTime = tempTime.AddMinutes(-myAlarms[position].Threshold);
            this.viewHolder.textViewThreshold.Text = tempTime.Hour.ToString().PadLeft(2, '0') + ":" + tempTime.Minute.ToString().PadLeft(2, '0');
            if (myAlarms[position].Description != null && myAlarms[position].Description.Trim().Length > 0)
            { this.viewHolder.textViewDescription.Text = myAlarms[position].Description; }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.element_myAlarm, parent, false);
            return new ViewHolder_MyAlarmAdapter(itemView, this);
        }
    }

    [Serializable]
    public class ViewHolder_MyAlarmAdapter : RecyclerView.ViewHolder
    {
        public TextView textViewIsAlarm { get; set; }
        public TextView textViewTime { get; set; }
        public TextView textViewThreshold { get; set; }
        public TextView textViewDescription { get; set; }
        public Button buttonEdit { get; set; }
        public Button buttonDelete { get; set; }
        public Adapter_MyAlarms adapter { get; set; }
        private List<Alarm> myAlarms { get; set; }

        public ViewHolder_MyAlarmAdapter(View itemView, Adapter_MyAlarms adapter) : base(itemView)
        {
            this.adapter = adapter;
            textViewIsAlarm = itemView.FindViewById<TextView>(Resource.Id.textViewIsAlarm);
            textViewTime = itemView.FindViewById<TextView>(Resource.Id.textViewTime);
            textViewThreshold = itemView.FindViewById<TextView>(Resource.Id.textViewThreshold);
            textViewDescription = itemView.FindViewById<TextView>(Resource.Id.textViewDescription);
            buttonEdit = itemView.FindViewById<Button>(Resource.Id.buttonEdit);
            buttonDelete = itemView.FindViewById<Button>(Resource.Id.buttonDelete);

            buttonDelete.Click += (e, o) => RemoveItem();
            buttonEdit.Click += (e, o) => EditItem();
        }

        /*
         * Pass reference to alarms 
         * to this class.        
         */
        public void SetMyAlarms(ref List<Alarm> myAlarms)
        {
            this.myAlarms = myAlarms;
        }

        /*
         * Delete button action:
         * deletes alarm localy and from server.
         */
        public void RemoveItem()
        {
            int position = AdapterPosition;
            Alarm alarm = myAlarms[position];
            ActivityMain.socialAlarm.RemoveAlarm(alarm);
            myAlarms.RemoveAt(position);
            adapter.NotifyItemRemoved(position);
        }

        /*
         * Edit button action:
         * edits alarm and sends update to server.
         */
        public void EditItem()
        {
            Intent intent = new Intent(adapter.activity, typeof(ActivityAddAlarm));
            intent.PutExtra("AlarmToEdit", JsonConvert.SerializeObject(myAlarms[AdapterPosition]));
            intent.PutExtra("AlarmPosition", AdapterPosition);
            adapter.activity.StartActivityForResult(intent, 1);
            adapter.NotifyItemChanged(AdapterPosition);
        }
    }
}