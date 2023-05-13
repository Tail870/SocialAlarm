using Microsoft.AspNetCore.Builder;

namespace Profiler_Service
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            if (Configs.RegistrationEnabled)
            { services.AddMvc(); }
            services.AddRazorPages();
            services.AddCors(options => options.AddPolicy("CorsPolicy", builder =>
            {
                builder.
                AllowAnyMethod().
                AllowAnyHeader().
                AllowAnyOrigin();
            }));
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
                endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}