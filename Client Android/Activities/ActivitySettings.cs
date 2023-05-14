using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.RecyclerView.Widget;
using Newtonsoft.Json;
using Xamarin.Essentials;

namespace Client_Android
{
    [Activity(Label = "@string/title_settings", Theme = "@style/AppTheme")]
    public class ActivitySettings : AppCompatActivity
    {
        private EditText editTextLogin { get; set; }
        private EditText editTextPassword { get; set; }
        private EditText editTextServer { get; set; }
        private Button buttonAdd { get; set; }
        private Button buttonSave { get; set; }
        private Button buttonCancel { get; set; }

        private RecyclerView recyclerView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_settings);

            recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            recyclerView.SetLayoutManager(new LinearLayoutManager(this)
            {
                // ReverseLayout = true,
                // StackFromEnd = true
            });
            ActivityMain.settings.MyRingtonesAdapter = new Adapter_MyRingtones(ActivityMain.settings.myRingtones, this);

            recyclerView.SetAdapter(ActivityMain.settings.MyRingtonesAdapter);
            ActivityMain.settings.MyRingtonesAdapter.NotifyDataSetChanged();

            editTextLogin = FindViewById<EditText>(Resource.Id.editTextLogin);
            editTextLogin.Text = Preferences.Get("Login", "");

            editTextPassword = FindViewById<EditText>(Resource.Id.editTextPassword);
            editTextPassword.Text = Preferences.Get("Password", "");

            editTextServer = FindViewById<EditText>(Resource.Id.editTextServer);
            editTextServer.Text = Preferences.Get("ServerAddress", "");

            buttonAdd = FindViewById<Button>(Resource.Id.buttonAdd);
            buttonAdd.Click += (e, o) => AddPersonalRingtone();
            buttonSave = FindViewById<Button>(Resource.Id.buttonSave);
            buttonSave.Click += (e, o) => SaveSettings();
            buttonCancel = FindViewById<Button>(Resource.Id.buttonCancel);
            buttonCancel.Click += (e, o) =>
            {
                ActivityMain.socialAlarm.GetMyRingtones();
                Finish();
            };
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
        {
            if (resultCode == Result.Ok)
            {
                switch (requestCode)
                {
                    case 2:
                        ActivityMain.socialAlarm.AddChangeRingtone(JsonConvert.DeserializeObject<Ringtone>(intent.GetStringExtra("RingtoneToEdit")));
                        recyclerView.ScrollToPosition(ActivityMain.settings.myRingtones.Count);
                        break;
                    case 3:
                        int position = intent.GetIntExtra("AlarmPosition", 0);
                        ActivityMain.socialAlarm.AddChangeRingtone(JsonConvert.DeserializeObject<Ringtone>(intent.GetStringExtra("RingtoneToEdit")));
                        ActivityMain.settings.MyRingtonesAdapter.NotifyItemChanged(position);
                        break;
                }
                ActivityMain.settings.MyRingtonesAdapter.NotifyDataSetChanged();
            }
        }

        private void AddPersonalRingtone()
        {
            Intent intent = new Intent(this, typeof(ActivityAddRingtone));
            StartActivityForResult(intent, 2);
        }

        private void SaveSettings()
        {
            Preferences.Set("Login", editTextLogin.Text);
            Preferences.Set("Password", editTextPassword.Text);
            Preferences.Set("ServerAddress", editTextServer.Text);
            Finish();
        }
    }
}