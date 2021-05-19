using Android.App;
using Android.Media;
using Android.OS;

namespace Client_Android.Activities
{
    [Activity(Label = "ActivityAlarming")]
    public class ActivityAlarming : Activity
    {
        public static Ringtone ringtone = RingtoneManager.GetRingtone(Application.Context, RingtoneManager.GetActualDefaultRingtoneUri(Application.Context, RingtoneType.Alarm));

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
        }
    }
}