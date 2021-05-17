﻿using Android.App;
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
        private List<Model_Alarm> myAlarms;
        private ViewHolder_MyAlarmAdapter viewHolder;
        private Activity activity;

        public Adapter_MyAlarms(ref List<Model_Alarm> MyAlarms, Activity activity)
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
                this.viewHolder.textViewIsAlarm.Text = Application.Context.Resources.GetString(Resource.String.isAlarm);
            else
                this.viewHolder.textViewIsAlarm.Text = Application.Context.Resources.GetString(Resource.String.isReminder);
            this.viewHolder.textViewTime.Text = myAlarms[position].Time.ToLocalTime().Hour.ToString().PadLeft(2, '0') + ":" + myAlarms[position].Time.ToLocalTime().Minute.ToString().PadLeft(2, '0');
            DateTimeOffset tempTime = myAlarms[position].Time.AddDays(1).AddMinutes(-myAlarms[position].Threshold);
            this.viewHolder.textViewThreshold.Text = tempTime.ToLocalTime().Hour.ToString().PadLeft(2, '0') + ":" + tempTime.ToLocalTime().Minute.ToString().PadLeft(2, '0');
            if (myAlarms[position].Description != null && myAlarms[position].Description.Trim().Length > 0)
                this.viewHolder.textViewDescription.Text = myAlarms[position].Description;
        }


        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.element_myAlarm, parent, false);
            return new ViewHolder_MyAlarmAdapter(itemView, activity);
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
            private List<Model_Alarm> myAlarms { get; set; }

            private Activity activity;

            public ViewHolder_MyAlarmAdapter(View itemView, Activity activity) : base(itemView)
            {
                this.activity = activity;
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
            public void SetMyAlarms(ref List<Model_Alarm> myAlarms) => this.myAlarms = myAlarms;

            /*
             * Delete button action:
             * deletes alarm localy and from server.
             */
            public void RemoveItem()
            {
                int position = AdapterPosition;
                Model_Alarm alarm = myAlarms[position];
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
                Intent intent = new Intent(activity, typeof(ActivityAddAlarm));
                intent.PutExtra("AlarmToEdit", JsonConvert.SerializeObject(myAlarms[AdapterPosition]));
                intent.PutExtra("AlarmPosition", AdapterPosition);
                activity.StartActivityForResult(intent, 1);
                adapter.NotifyItemChanged(AdapterPosition);
            }
        }
    }
}