using MassTransit;

namespace Colorful.Discord
{
    public class ColorIntentConsumerDefinition : ConsumerDefinition<ColorIntentConsumer>
    {
        public ColorIntentConsumerDefinition()
        {
            EndpointName = "colorful-intent";
            ConcurrentMessageLimit = 8;
        }

        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<ColorIntentConsumer> consumerConfigurator)
        {
            endpointConfigurator.UseMessageRetry(r => r.Intervals(100, 200, 500, 1000));
            endpointConfigurator.UseInMemoryOutbox();
        }

    }
}
