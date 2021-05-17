using Android.App;
using Android.Content;
using Android.Media;
using Android.Net;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Client_Android
{
    public class Adapter_OthersRingtones : RecyclerView.Adapter
    {
        protected List<Model_Ringtone> Ringtones;
        ViewHolder_Ringtones viewHolder;
        Model_Alarm alarm;


        public Adapter_OthersRingtones(List<Model_Ringtone> Ringtones, Model_Alarm alarm)
        {
            this.Ringtones = Ringtones;
            this.alarm = alarm;
        }

        public override int ItemCount => Ringtones.Count;

        /*
         * Custom binding element in: List<AlarmLogModel> AlarmsLogs
         * to RecycleView (on activity).         
         */
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            this.viewHolder = (ViewHolder_Ringtones)viewHolder;
            this.viewHolder.adapter = this;

            this.viewHolder.textViewRingtonename.Text = Ringtones[position].RingtoneName;
            if (Ringtones[position].Description != null && Ringtones[position].Description.Trim().Length > 0)
                this.viewHolder.textViewDescription.Text = Ringtones[position].Description;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.element_othersRingtone, parent, false);
            return new ViewHolder_Ringtones(itemView);
        }

        [System.Serializable]
        public class ViewHolder_Ringtones : RecyclerView.ViewHolder
        {
            public TextView textViewRingtonename { get; set; }
            public TextView textViewDescription { get; set; }
            public Button buttonRing { get; set; }
            public Adapter_OthersRingtones adapter { get; set; }

            public ViewHolder_Ringtones(View itemView) : base(itemView)
            {
                textViewRingtonename = itemView.FindViewById<TextView>(Resource.Id.textViewRingtonename);
                textViewDescription = itemView.FindViewById<TextView>(Resource.Id.textViewDescription);
                buttonRing = itemView.FindViewById<Button>(Resource.Id.buttonRing);

                buttonRing.Click += (e, o) => Ring();
            }

            /*
             * Ring button action:
             * plays ringtone remotely.
             */
            public void Ring()
            {
                ActivityMain.socialAlarm.RingAlarm(adapter.alarm.ID, adapter.Ringtones[AdapterPosition].ID);
            }
        }
    }
}