using Colorful.Common;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;
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
            services.AddSingleton(x => new DiscordClient(new DiscordConfiguration()
            {
                Token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN"),
                Intents = DiscordIntents.Guilds | DiscordIntents.GuildMembers, // Discord does not know how to manage their intents properly.
                TokenType = TokenType.Bot,
                AlwaysCacheMembers = false,
            }));
            services.AddScoped<ColorIntentConsumer>();
        }

        private async Task Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            CheckUnused(sender);

            await sender.UpdateStatusAsync(new DiscordActivity() { ActivityType = ActivityType.Playing, Name = "@ clrful.xyz"});
            Console.WriteLine("Ready. Providing color roles for " + sender.Guilds.Count + " guilds");
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
                var guild = await client.GetGuildAsync(gid, true);
                await CleanGuildRoles(guild);
            }
            Console.WriteLine($"Removed unused color roles.");
        }

        private async Task CleanGuildRoles(DiscordGuild guild)
        {
            List<DiscordRole> allColorRoles = guild.Roles.Values
                .Where(x => Color.HEX_COLOR_REGEX.IsMatch(x.Name))
                .ToList();

            List<DiscordRole> allUsedRoles = guild.Members.Values
                .SelectMany(m => m.Roles)
                .Where(x => Color.HEX_COLOR_REGEX.IsMatch(x.Name))
                .Distinct()
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
    }
}
