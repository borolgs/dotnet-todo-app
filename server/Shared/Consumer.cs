using System.Threading.Channels;

namespace App.Shared;

public static class ServiceExtensions {
  public static void AddChannelBasedConsumer<TEvent, TConsumer>(this IServiceCollection services)
      where TConsumer : ChannelConsumerBase<TEvent> {
    services.AddHostedService<TConsumer>();
  }
}

public abstract class ChannelConsumerBase<TEvent>(Channel<TEvent> channel, IServiceProvider serviceProvider) : BackgroundService {
  private readonly Channel<TEvent> channel = channel;
  public readonly IServiceProvider serviceProvider = serviceProvider;

  protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
    var reader = channel.Reader;

    while (await reader.WaitToReadAsync(stoppingToken)) {
      while (reader.TryRead(out var @event)) {
        await ProcessEventAsync(@event, stoppingToken);
      }
    }
  }

  public abstract Task ProcessEventAsync(TEvent @event, CancellationToken stoppingToken);
}
