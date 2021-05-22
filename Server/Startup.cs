using Bazinga.AspNetCore.Authentication.Basic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace Social_Alarm_Server
{
    public class Startup
    {
        private readonly DataBridge dataBridge = new();

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DataBridge.DataBaseContext>();
            services.AddCors(options => options.AddPolicy("CorsPolicy", builder =>
            {
                builder.
                AllowAnyMethod().
                AllowAnyHeader().
                AllowAnyOrigin();
            }));
            services.AddSignalR();
            services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme).AddBasicAuthentication(credentials =>
            Task.FromResult(dataBridge.AuthCheck(credentials.username, credentials.password)));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            { app.UseDeveloperExceptionPage(); }
            app.UseAuthentication();
            app.UseCors("CorsPolicy");
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context => { await context.Response.WriteAsync("Hello World!"); });
                app.UseEndpoints(endpoints => endpoints.MapHub<SocialAlarm_Server>(Configs.Endpoint));
            });
        }
    }
}