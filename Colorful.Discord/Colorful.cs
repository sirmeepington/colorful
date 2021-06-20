using Colorful.Common;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.DependencyInjection;
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
        static void Main(string[] args)
        {
            Colorful colorful = new Colorful();
            colorful.MainAsync().GetAwaiter().GetResult();
        }

        private async Task MainAsync()
        {
            DiscordClient discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN"),
                Intents = DiscordIntents.All,
                TokenType = TokenType.Bot,
                AlwaysCacheMembers = false
            });

            ServiceCollection services = new ServiceCollection();
            services.AddMassTransit(opt =>
            {
                opt.UsingRabbitMq((context, config) => InitRabbit(context, config));
            });
            services.AddSingleton(discord);
            services.AddScoped<ColorIntentConsumer>();
            ServiceProvider provider = services.BuildServiceProvider();

            var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                EnableDms = false,
                StringPrefixes = new[] { "c!" },
                EnableMentionPrefix = false,
                Services = provider
            });

            commands.RegisterCommands(Assembly.GetExecutingAssembly());
            commands.SetHelpFormatter<CustomHelpFormatter>();

            discord.Ready += Ready;

            IBusControl bus = provider.GetRequiredService<IBusControl>();
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

        private Task Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            CheckUnused(sender);
            return Task.CompletedTask;
        }

        private async Task CheckUnused(DiscordClient client)
        {
            await Task.Delay(TimeSpan.FromSeconds(30));
            while (true)
            {
                Console.WriteLine("Attempting to remove unused color roles.");
                try
                {
                    await RemoveUnused(client);
                } catch (Exception ex)
                {
                    Console.WriteLine("Removing unused colors failed:");
                    Console.WriteLine(ex.Message);
                }
                Console.WriteLine("Finished removing unused roles.");
                await Task.Delay(TimeSpan.FromMinutes(5));
            }
        }


        private async Task RemoveUnused(DiscordClient client)
        {
            foreach(ulong gid in client.Guilds.Keys)
            {
                var guild = await client.GetGuildAsync(gid);
                await CleanGuildRoles(guild);
            }
            Console.WriteLine($"Removed unused color roles.");
        }

        private Task CleanGuildRoles(DiscordGuild guild)
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

            return Task.WhenAll(unusedRoles.Select(r => r.DeleteAsync()));
        }

        private void InitRabbit(IBusRegistrationContext context, IRabbitMqBusFactoryConfigurator config)
        {
            config.Host(Environment.GetEnvironmentVariable("RABBIT_HOST"),"/",settings =>
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
