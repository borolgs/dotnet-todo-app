using System.Threading.Channels;
using App.Db;
using App.Shared;

namespace App.Todos;

class TodoUserEventConsumer(
  Channel<UserEvent> channel,
  IServiceProvider serviceProvider
) : ChannelConsumerBase<UserEvent>(channel, serviceProvider) {
  public override async Task ProcessEventAsync(UserEvent @event, CancellationToken stoppingToken) {
    using var scope = serviceProvider.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<TodoUserEventConsumer>>();

    logger.LogInformation("Consume {event}", @event);
    var db = scope.ServiceProvider.GetRequiredService<DbCtx>();

    if (@event is UserCreated userCreatedEvent) {
      var Id = userCreatedEvent.User.Id;
      var editor = new Editor {
        Id = Id,
      };
      db.Editors.Add(editor);
      await db.SaveChangesAsync(stoppingToken);
      logger.LogInformation("Editor {Id} Created", Id);
    }
  }
}