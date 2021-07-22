using Colorful.Common;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.DependencyInjection;
using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Colorful.Discord
{
    public class Colorful
    {
        public static void Main(string[] args)
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
            }))
            {
                ServiceCollection services = new ServiceCollection();
                ConfigureServices(services);
                ServiceProvider serviceProvider = services.BuildServiceProvider();

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
        }

        /// <summary>
        /// Initialises DSharpPlus' CommandsNext module.
        /// </summary>
        /// <param name="provider"></param>
        private void InitDiscordCommands(ServiceProvider provider)
        {
            DiscordClient discord = provider.GetRequiredService<DiscordClient>();
            CommandsNextExtension commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                EnableDms = false,
                StringPrefixes = new[] { "c!" },
                EnableMentionPrefix = false,
                Services = provider
            });

            commands.RegisterCommands(Assembly.GetExecutingAssembly());
            commands.SetHelpFormatter<CustomHelpFormatter>();
        }

        /// <summary>
        /// Configures the services for the <see cref="ServiceCollection"/> given.
        /// </summary>
        private void ConfigureServices(ServiceCollection services)
        {
            services.AddMassTransit(opt =>
            {
                opt.UsingRabbitMq((context, config) => InitRabbit(context, config));
            });
            services.AddSingleton<DiscordClient>(x => new DiscordClient(new DiscordConfiguration()
            {
                Token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN"),
                Intents = DiscordIntents.All, // Discord does not know how to manage their intents properly.
                TokenType = TokenType.Bot,
                AlwaysCacheMembers = false
            }));
            services.AddScoped<ColorIntentConsumer>();
        }

        private async Task Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            CheckUnused(sender);

            await sender.UpdateStatusAsync(new DiscordActivity() { ActivityType = ActivityType.Playing, Name = "@ clrful.xyz"});
        }

        private async Task CheckUnused(DiscordClient client)
        {
            while (true)
            {
                Console.WriteLine("Attempting to remove unused color roles.");
                try
                {
                    await RemoveUnused(client);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Removing unused colors failed: {ex.Message}");
                }
                Console.WriteLine("Finished removing unused roles.");
                await Task.Delay(TimeSpan.FromMinutes(5));
            }
        }

        private async Task RemoveUnused(DiscordClient client)
        {
            foreach (ulong gid in client.Guilds.Keys)
            {
                var guild = await client.GetGuildAsync(gid);
                await CleanGuildRoles(guild);
            }
            Console.WriteLine($"Removed unused color roles.");
        }

        private async Task CleanGuildRoles(DiscordGuild guild)
        {
            Console.WriteLine($"Looking at guild {guild.Name}");

            List<DiscordRole> allColorRoles = guild.Roles.Values
                .Where(x => Color.HEX_COLOR_REGEX.IsMatch(x.Name))
                .ToList();

            List<DiscordRole> allUsedRoles = guild.Members.Values
                .SelectMany(m => m.Roles)
                .Where(x => Color.HEX_COLOR_REGEX.IsMatch(x.Name))
                .ToList();

            IEnumerable<DiscordRole> unusedRoles = allColorRoles.Except(allUsedRoles);

            foreach(var r in unusedRoles)
            {
                try
                {
                    await r.DeleteAsync();
                } catch (InvalidOperationException ex)
                {
                    SentrySdk.CaptureException(ex);
                } catch (DSharpPlus.Exceptions.UnauthorizedException)
                {
                    SentrySdk.CaptureMessage($"Attempted to remove role without permission: {r} in {guild.Name} ({guild.Id})", SentryLevel.Warning);
                    continue;
                }
            } 
        }

        private void InitRabbit(IBusRegistrationContext context, IRabbitMqBusFactoryConfigurator config)
        {
            config.Host(Environment.GetEnvironmentVariable("RABBIT_HOST"), "/", settings =>
            {
                settings.Username(Environment.GetEnvironmentVariable("RABBIT_USER"));
                settings.Password(Environment.GetEnvironmentVariable("RABBIT_PASS"));
            });

            config.ReceiveEndpoint(endpoint =>
            {
                endpoint.Bind<IColorIntent>();
                endpoint.Consumer<ColorIntentConsumer>(context);
            });
        }
    }
}
