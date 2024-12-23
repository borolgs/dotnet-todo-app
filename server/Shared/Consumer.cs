using System.Threading.Channels;

namespace App.Shared;

public static class ServiceExtensions {
  public static void AddChannelBasedProcessing<TEvent, TProcessor, TConsumer>(this IServiceCollection services)
      where TProcessor : class, IEventProcessor<TEvent>
      where TConsumer : ChannelConsumerBase<TEvent> {
    services.AddScoped<IEventProcessor<TEvent>, TProcessor>();
    services.AddHostedService<TConsumer>();
  }
}

public abstract class ChannelConsumerBase<TEvent>(Channel<TEvent> channel, IServiceProvider serviceProvider) : BackgroundService {
  private readonly Channel<TEvent> channel = channel;
  private readonly IServiceProvider serviceProvider = serviceProvider;

  protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
    var reader = channel.Reader;

    while (await reader.WaitToReadAsync(stoppingToken)) {
      while (reader.TryRead(out var @event)) {
        await ProcessEventAsync(@event, stoppingToken);
      }
    }
  }

  private async Task ProcessEventAsync(TEvent @event, CancellationToken stoppingToken) {
    using var scope = serviceProvider.CreateScope();
    var processor = scope.ServiceProvider.GetRequiredService<IEventProcessor<TEvent>>();
    await processor.ProcessAsync(@event, stoppingToken);
  }
}

public interface IEventProcessor<in TEvent> {
  Task ProcessAsync(TEvent @event, CancellationToken stoppingToken);
}

