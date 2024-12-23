using System.Threading.Channels;
using App.Db;
using App.Shared;

namespace App.Todos;

class TodoUserEventConsumer(
  Channel<UserEvent> channel,
  IServiceProvider serviceProvider
) : ChannelConsumerBase<UserEvent>(channel, serviceProvider) { }

class TodoUserEventsProcessor(IServiceProvider serviceProvider) : IEventProcessor<UserEvent> {
  private readonly IServiceProvider serviceProvider = serviceProvider;

  public async Task ProcessAsync(UserEvent userEvent, CancellationToken stoppingToken) {
    using var scope = serviceProvider.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<TodoUserEventsProcessor>>();

    logger.LogInformation($"Consume {userEvent}");
    var db = scope.ServiceProvider.GetRequiredService<DbCtx>();

    if (userEvent is UserCreated userCreatedEvent) {
      var Id = userCreatedEvent.User.Id;
      var editor = new Editor {
        Id = Id,
      };
      db.Editors.Add(editor);
      await db.SaveChangesAsync(stoppingToken);
      logger.LogInformation($"Editor {Id} Created");
    }
  }
}