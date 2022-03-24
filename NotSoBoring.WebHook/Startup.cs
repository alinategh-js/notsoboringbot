using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotSoBoring.DataAccess;
using NotSoBoring.Matchmaking;
using NotSoBoring.Matchmaking.Users;
using NotSoBoring.WebHook.Services;
using NotSoBoring.WebHook.Services.Handlers;
using System.Threading;
using Telegram.Bot;

namespace NotSoBoring.WebHook
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            BotConfig = Configuration.GetSection("BotConfiguration").Get<BotConfiguration>();
        }

        public IConfiguration Configuration { get; }
        private BotConfiguration BotConfig { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHostedService<ConfigureWebhook>();

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            services.AddSingleton(cancellationTokenSource);

            services.AddHttpClient("tgwebhook")
               .AddTypedClient<ITelegramBotClient>(httpClient
                   => new TelegramBotClient(BotConfig.BotToken, httpClient));

            services.AddScoped<HandleUpdateService>();
            services.AddScoped<CommandHandler>();
            services.AddScoped<SessionHandler>();
            services.AddSingleton<UserService>();
            services.AddSingleton<MatchingEngine>();

            services.AddControllers()
                .AddNewtonsoftJson();

            var mainDbConnectionString = Configuration.GetConnectionString("MainDbContext");
            services.AddDbContext<MainDbContext>(x => x.UseSqlServer(mainDbConnectionString).EnableDetailedErrors(), ServiceLifetime.Transient, ServiceLifetime.Transient);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                var token = BotConfig.BotToken;
                endpoints.MapControllerRoute(name: "tgwebhook",
                                         pattern: $"bot/{token}",
                                         new { controller = "Webhook", action = "Post" });

                endpoints.MapControllers();
            });
        }
    }
}
