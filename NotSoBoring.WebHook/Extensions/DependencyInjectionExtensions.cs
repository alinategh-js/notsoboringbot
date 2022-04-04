using NotSoBoring.WebHook.Services;
using Microsoft.Extensions.DependencyInjection;
using NotSoBoring.Matchmaking.Users;
using NotSoBoring.Matchmaking;
using System.Threading;
using NotSoBoring.WebHook.Services.Handlers.MessageHandlers;
using NotSoBoring.WebHook.Services.Handlers.CallbackQueryHandlers;

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
            services.AddScoped<CallbackQueryHandler>();
            services.AddScoped<ContactService>();

            return services;
        }

        public static IServiceCollection AddSingletonServices(this IServiceCollection services)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            services.AddSingleton(cancellationTokenSource);
            services.AddSingleton<MatchingEngine>();
            services.AddSingleton<UserService>();

            return services;
        }

        public static IServiceCollection AddTransientServices(this IServiceCollection services)
        {
            

            return services;
        }
    }
}
