using Android.App;
using Android.Content;
using Android.Media;
using Android.Net;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using Newtonsoft.Json;
using Xamarin.Essentials;

namespace Client_Android
{

    [Activity(Label = "@string/title_ringtone_editor", Theme = "@style/AppTheme")]
    public class ActivityAddRingtone : AppCompatActivity
    {
        public Button buttonSave { get; set; }
        public Button buttonCancel { get; set; }
        public Button buttonChooseRingtone { get; set; }
        public TextView textViewUsername { get; set; }
        public EditText editTextRingtoneName { get; set; }
        public EditText editTextDescription { get; set; }
        public TextView textViewRingtoneURI { get; set; }

        private Ringtone ringtone;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_addRingtone);

            textViewUsername = FindViewById<TextView>(Resource.Id.textViewUsername);
            textViewUsername.Text = Preferences.Get("Login", "");
            editTextRingtoneName = FindViewById<EditText>(Resource.Id.editTextRingtoneName);
            editTextDescription = FindViewById<EditText>(Resource.Id.editTextDescription);
            textViewRingtoneURI = FindViewById<TextView>(Resource.Id.textViewRingtoneURI);

            buttonSave = FindViewById<Button>(Resource.Id.buttonSave);
            buttonCancel = FindViewById<Button>(Resource.Id.buttonCancel);
            buttonChooseRingtone = FindViewById<Button>(Resource.Id.buttonChooseRingtone);

            buttonSave.Click += (e, o) => SaveChanges();
            buttonCancel.Click += (e, o) => Finish();
            buttonChooseRingtone.Click += (e, o) => ChooseRingtone();

            string ringtoneJSON = Intent.GetStringExtra("RingtoneToEdit");
            if (ringtoneJSON != null)
            {
                ringtone = JsonConvert.DeserializeObject<Ringtone>(ringtoneJSON);
                editTextRingtoneName.Text = ringtone.RingtoneName;
                editTextDescription.Text = ringtone.Description;
                textViewRingtoneURI.Text = ringtone.File;
            }
            else
            { ringtone = new Ringtone(); }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
        {
            switch (requestCode)
            {
                case 77:
                    if (resultCode == Result.Ok)
                    {
                        ringtone.File = intent.GetParcelableExtra(RingtoneManager.ExtraRingtonePickedUri).ToString();
                        if (ringtone.File != null || ringtone.File.Length > 0)
                        {
                            editTextRingtoneName.Text = RingtoneManager.GetRingtone(this, Uri.Parse(ringtone.File)).GetTitle(this);
                            textViewRingtoneURI.Text = ringtone.File;
                        }
                    }
                    break;
            }
        }

        private void ChooseRingtone()
        {
            Intent intent = new Intent(RingtoneManager.ActionRingtonePicker);
            StartActivityForResult(intent, 77);
        }

        private void SaveChanges()
        {
            if (ringtone == null)
            { ringtone = new Ringtone(); }
            ringtone.User = Preferences.Get("Login", "");
            ringtone.RingtoneName = editTextRingtoneName.Text;
            ringtone.Description = editTextDescription.Text;
            if (ringtone.RingtoneName.Length > 0 && ringtone.File != null)
            {
                Intent.PutExtra("RingtoneToEdit", JsonConvert.SerializeObject(ringtone));
                SetResult(Result.Ok, Intent);
                Finish();
            }
        }
    }
}