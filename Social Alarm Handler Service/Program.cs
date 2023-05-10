using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Social_Alarm
{
    public class Program
    {
        public static Configs configs = new Configs();

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.UseUrls(Configs.URLs);
            });
        }
    }
}