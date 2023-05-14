using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using System.Collections.Generic;

namespace Client_Android
{
    public class Adapter_OthersRingtones : RecyclerView.Adapter
    {
        private ViewHolder_OthersRingtones viewHolder;
        public List<Ringtone> Ringtones { get; }
        public Alarm alarm { get; }

        public Adapter_OthersRingtones(List<Ringtone> Ringtones, Alarm alarm)
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
            this.viewHolder = (ViewHolder_OthersRingtones)viewHolder;
            this.viewHolder.textViewRingtonename.Text = Ringtones[position].RingtoneName;
            if (Ringtones[position].Description != null && Ringtones[position].Description.Trim().Length > 0)
            { this.viewHolder.textViewDescription.Text = Ringtones[position].Description; }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.element_othersRingtone, parent, false);
            return new ViewHolder_OthersRingtones(itemView, this);
        }
    }

    [System.Serializable]
    public class ViewHolder_OthersRingtones : RecyclerView.ViewHolder
    {
        public TextView textViewRingtonename { get; set; }
        public TextView textViewDescription { get; set; }
        public Button buttonRing { get; set; }
        private Adapter_OthersRingtones adapter { get; set; }

        public ViewHolder_OthersRingtones(View itemView, Adapter_OthersRingtones adapter) : base(itemView)
        {
            this.adapter = adapter;
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