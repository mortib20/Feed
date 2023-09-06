using ADSBRouter;
using Feed.ADSBRouter;

namespace Feed.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //builder.Services.AddControllersWithViews();
            builder.Services.AddSingleton<RouterManager>();
            builder.Services.AddHostedService<RouterManagerService>();

            var app = builder.Build();

            //app.MapControllerRoute(name: "areas", pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}