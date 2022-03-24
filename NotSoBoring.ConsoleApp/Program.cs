using NotSoBoring.DataAccess;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using NotSoBoring.ConsoleApp.Handlers;
using System.Threading.Tasks;

namespace NotSoBoring.ConsoleApp
{
    class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var mainDbConnectionString = hostContext.Configuration.GetConnectionString("MainDbContext");

                    services.AddDbContext<MainDbContext>(x => x.UseSqlServer(mainDbConnectionString).EnableDetailedErrors(), ServiceLifetime.Transient, ServiceLifetime.Transient);

                    var cancellationTokenSource = new CancellationTokenSource();
                    services.AddSingleton(cancellationTokenSource);

                    var mainHandler = new MainHandler(cancellationTokenSource, hostContext.Configuration);
                    
                    Task.Factory.StartNew(async (obj) => await mainHandler.Start(), null, cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
                });
    }
}
