namespace Social_Alarm
{
    public class Configs
    {
        //TODO Config loading from file.

        public static bool RegistrationEnabled { set; get; } = true;
        public static int DelayTime { set; get; } = 0;
        public static string Endpoint { set; get; } = "/SocialAlarm";
        public static string[] URLs { set; get; } = {
            "http://0.0.0.0:870"};

        public class Database
        {
            public static string Host { set; get; } = "192.168.2.7";
            public static string Port { set; get; } = "5432";
            public static string DatabaseName { set; get; } = "social_alarm";
            public static string Username { set; get; } = "postgres";
            public static string Password { set; get; } = "3585";
        }
    }
}
