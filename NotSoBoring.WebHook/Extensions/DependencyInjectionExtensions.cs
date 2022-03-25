using NotSoBoring.WebHook.Services;
using NotSoBoring.WebHook.Services.Handlers;
using Microsoft.Extensions.DependencyInjection;
using NotSoBoring.Matchmaking.Users;
using NotSoBoring.Matchmaking;
using System.Threading;

namespace NotSoBoring.WebHook.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddScopedServices(this IServiceCollection services)
        {
            services.AddScoped<HandleUpdateService>();
            services.AddScoped<CommandHandler>();
            services.AddScoped<SessionHandler>();
            services.AddScoped<GeneralHandler>();

            return services;
        }

        public static IServiceCollection AddSingletonServices(this IServiceCollection services)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            services.AddSingleton(cancellationTokenSource);
            services.AddSingleton<UserService>();
            services.AddSingleton<MatchingEngine>();

            return services;
        }

        public static IServiceCollection AddTransientServices(this IServiceCollection services)
        {
            

            return services;
        }
    }
}
