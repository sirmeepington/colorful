using Colorful.Common;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sentry;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Colorful.Discord
{
    public class Colorful
    {

        private ILogger<Colorful> _logger;

        public static void Main()
        {
            Colorful colorful = new Colorful();
            colorful.MainAsync().GetAwaiter().GetResult();
        }

        private async Task MainAsync()
        {
            using (SentrySdk.Init(o =>
            {
                o.Dsn = Environment.GetEnvironmentVariable("SENTRY_DSN");
                o.TracesSampleRate = 0.4f;
            }));
            ServiceCollection services = new();
            ConfigureServices(services);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            _logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<Colorful>();

            var discord = serviceProvider.GetRequiredService<DiscordClient>();
            IBusControl bus = serviceProvider.GetRequiredService<IBusControl>();

            InitDiscordCommands(serviceProvider);
            discord.Ready += Ready;

            try
            {
                await bus.StartAsync();
                await discord.ConnectAsync();
                await Task.Delay(Timeout.Infinite);
            }
            finally
            {
                await bus.StopAsync();
            }
        }

        /// <summary>
        /// Initialises DSharpPlus' CommandsNext module.
        /// </summary>
        /// <param name="provider"></param>
        private void InitDiscordCommands(ServiceProvider provider)
        {
            DiscordClient discord = provider.GetRequiredService<DiscordClient>();
            var commands = discord.UseSlashCommands(new SlashCommandsConfiguration()
            {
                Services = provider
            });

            commands.RegisterCommands<ColorfulCommands>();
        }

        /// <summary>
        /// Configures the services for the <see cref="ServiceCollection"/> given.
        /// </summary>
        private void ConfigureServices(ServiceCollection services)
        {
            services.AddMassTransit(opt =>
            {
                opt.AddConsumer<ColorIntentConsumer>(typeof(ColorIntentConsumerDefinition));
                opt.UsingRabbitMq((context, config) =>
                {
                    config.ConfigureEndpoints(context);
                    config.Host(Environment.GetEnvironmentVariable("RABBIT_HOST"), Environment.GetEnvironmentVariable("RABBIT_VHOST") ?? "/", settings =>
                    {
                        settings.Username(Environment.GetEnvironmentVariable("RABBIT_USER"));
                        settings.Password(Environment.GetEnvironmentVariable("RABBIT_PASS"));
                    });
                });
            });

            Log.Logger = new LoggerConfiguration().Enrich.FromLogContext().WriteTo.Console().CreateLogger();
            services.AddLogging(x =>
            {
                x.AddSerilog(dispose: true);
            });

            services.AddSingleton(x => new DiscordClient(new DiscordConfiguration()
            {
                Token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN"),
                Intents = DiscordIntents.Guilds | DiscordIntents.GuildMembers, // Discord does not know how to manage their intents properly.
                TokenType = TokenType.Bot,
                AlwaysCacheMembers = false,
            }));
            services.AddScoped<ColorIntentConsumer>();
        }

        private async Task Ready(DiscordClient sender, ReadyEventArgs e)
        {
            await sender.UpdateStatusAsync(new DiscordActivity() { ActivityType = ActivityType.Playing, Name = "@ clrful.xyz"});
            _logger.LogInformation("Ready. Providing color roles for {guildCount} guilds", sender.Guilds.Count);
        }
        
    }
}
