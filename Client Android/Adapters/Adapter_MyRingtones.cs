﻿using Android.App;
using Android.Content;
using Android.Media;
using Android.Net;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;

namespace Client_Android
{
    public class Adapter_MyRingtones : RecyclerView.Adapter
    {
        protected List<Model_Ringtone> Ringtones;
        private ViewHolder_Ringtones viewHolder;

        private Activity activity;

        public Adapter_MyRingtones(List<Model_Ringtone> Ringtones, Activity activity)
        {
            this.Ringtones = Ringtones;
            this.activity = activity;
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
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.element_Ringtone, parent, false);
            return new ViewHolder_Ringtones(itemView);
        }

        [System.Serializable]
        public class ViewHolder_Ringtones : RecyclerView.ViewHolder
        {
            public TextView textViewRingtonename { get; set; }
            public TextView textViewDescription { get; set; }
            public Button buttonEdit { get; set; }
            public Button buttonDelete { get; set; }
            public Button buttonTest { get; set; }
            public Adapter_MyRingtones adapter { get; set; }

            public ViewHolder_Ringtones(View itemView) : base(itemView)
            {
                textViewRingtonename = itemView.FindViewById<TextView>(Resource.Id.textViewRingtonename);
                textViewDescription = itemView.FindViewById<TextView>(Resource.Id.textViewDescription);
                buttonEdit = itemView.FindViewById<Button>(Resource.Id.buttonEdit);
                buttonDelete = itemView.FindViewById<Button>(Resource.Id.buttonDelete);
                buttonTest = itemView.FindViewById<Button>(Resource.Id.buttonTest);

                buttonTest.Text = Application.Context.Resources.GetString(Resource.String.Play);

                buttonDelete.Click += (e, o) => RemoveItem(AdapterPosition);
                buttonEdit.Click += (e, o) => EditItem(AdapterPosition);
                buttonTest.Click += (e, o) => TestItem(AdapterPosition);
            }

            /*
             * Delete button action:
             * deletes ringtone localy and from server.
             */
            public void RemoveItem(int position)
            {
                StopPlaying();
                ActivityMain.settings.RemoveRingtone(adapter.Ringtones[position]);
                adapter.NotifyItemRemoved(position);
                adapter.Ringtones.RemoveAt(position);
            }

            /*
             * Edit button action:
             * edits ringtone and sends update to server.
             */
            public void EditItem(int position)
            {
                StopPlaying();
                Intent intent = new Intent(adapter.activity, typeof(ActivityAddRingtone));
                intent.PutExtra("RingtoneToEdit", JsonConvert.SerializeObject(adapter.Ringtones[position]));
                adapter.activity.StartActivityForResult(intent, 3);
            }
            /*
             * Test button action:
             * edits ringtone and sends update to server.
             */
            Ringtone ringtone = null;
            public void TestItem(int position)
            {
                if (ringtone == null)
                    ringtone = RingtoneManager.GetRingtone(adapter.activity, Uri.Parse(adapter.Ringtones[position].File));
                if (ringtone.IsPlaying)
                    StopPlaying();
                else
                {
                    ringtone = RingtoneManager.GetRingtone(adapter.activity, Uri.Parse(adapter.Ringtones[position].File));
                    buttonTest.Text = Application.Context.Resources.GetString(Resource.String.Stop);
                    ringtone.Play();
                }
                Thread OnStop = new Thread(() =>
                {
                    while (ringtone.IsPlaying) { }
                    adapter.activity.RunOnUiThread(StopPlaying);
                });
                OnStop.Start();
            }

            void StopPlaying()
            {
                if (ringtone != null)
                    ringtone.Stop();
                buttonTest.Text = Application.Context.Resources.GetString(Resource.String.Play);
            }
        }
    }
}