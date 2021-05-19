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
        private Adapter_MyRingtones MyRingtonesAdapter { get; set; }

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
            MyRingtonesAdapter = new Adapter_MyRingtones(ActivityMain.settings.myRingtones, this);

            recyclerView.SetAdapter(MyRingtonesAdapter);
            MyRingtonesAdapter.NotifyDataSetChanged();

            editTextLogin = FindViewById<EditText>(Resource.Id.editTextLogin);
            editTextLogin.Text = Preferences.Get("Login", "");

            editTextPassword = FindViewById<EditText>(Resource.Id.editTextPassword);
            editTextPassword.Text = Preferences.Get("Password", "");

            editTextServer = FindViewById<EditText>(Resource.Id.editTextServer);
            editTextServer.Text = Preferences.Get("ServerAddress", "");

            buttonAdd = FindViewById<Button>(Resource.Id.buttonAdd);
            buttonAdd.Click += (e, o) => AddPersonalRingtone();
            buttonSave = FindViewById<Button>(Resource.Id.buttonSave);
            buttonSave.Click += (e, o) => SaveSettingsAsync();
            buttonCancel = FindViewById<Button>(Resource.Id.buttonCancel);
            buttonCancel.Click += (e, o) => Finish();
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
        {
            if (resultCode == Result.Ok)
            {
                switch (requestCode)
                {
                    case 2:
                        ActivityMain.settings.AddRingtone(JsonConvert.DeserializeObject<Model_Ringtone>(intent.GetStringExtra("RingtoneToEdit")));
                        recyclerView.ScrollToPosition(ActivityMain.settings.myRingtones.Count + 1);
                        break;
                    case 3:
                        int position = intent.GetIntExtra("AlarmPosition", 0);
                        ActivityMain.settings.AddRingtone(JsonConvert.DeserializeObject<Model_Ringtone>(intent.GetStringExtra("RingtoneToEdit")));
                        MyRingtonesAdapter.NotifyItemChanged(position);
                        break;
                }
                MyRingtonesAdapter.NotifyDataSetChanged();
            }
        }

        private void AddPersonalRingtone()
        {
            Intent intent = new Intent(this, typeof(ActivityAddRingtone));
            StartActivityForResult(intent, 2);
        }

        private async System.Threading.Tasks.Task SaveSettingsAsync()
        {
            Preferences.Set("Login", editTextLogin.Text);
            Preferences.Set("Password", editTextPassword.Text);
            Preferences.Set("ServerAddress", editTextServer.Text);
            Finish();
        }
    }
}