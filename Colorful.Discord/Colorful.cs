using Colorful.Common;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Colorful.Discord
{
    public class Colorful
    {
        

        private ColorfulCommands _events;

        static void Main(string[] args)
        {
            Colorful colorful = new Colorful();
            colorful.MainAsync().GetAwaiter().GetResult();
        }

        private async Task MainAsync()
        {

            _events = new ColorfulCommands();
            DiscordShardedClient discord = new DiscordShardedClient(new DiscordConfiguration()
            {
                Token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN"),
            });

            ServiceCollection services = new ServiceCollection();
            services.AddMassTransit(opt =>
            {
                opt.UsingRabbitMq((context, config) => InitRabbit(context, config));
            });
            services.AddSingleton(discord);
            services.AddScoped<ColorIntentConsumer>();
            ServiceProvider provider = services.BuildServiceProvider();

            var commands = await discord.UseCommandsNextAsync(new CommandsNextConfiguration()
            {
                EnableDms = false,
                StringPrefixes = new[] { "c!" },
                EnableMentionPrefix = false,
                Services = provider
            });

            foreach (CommandsNextExtension shardedCommand in commands.Values)
            {
                shardedCommand.RegisterCommands(Assembly.GetExecutingAssembly());
                shardedCommand.SetHelpFormatter<CustomHelpFormatter>();
            }

            IBusControl bus = provider.GetRequiredService<IBusControl>();
            try
            {
                await bus.StartAsync();
                await discord.StartAsync();
                await Task.Delay(Timeout.Infinite);
            }
            finally
            {
                await bus.StopAsync();
            }

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
