using System.Threading.Channels;
using App.Db;

namespace App.Todos;

class TodoUserEventsProcessor(IServiceProvider serviceProvider) : IUserEventProcessor {
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

class TodoUserEventConsumer(Channel<UserEvent> channel, TodoUserEventsProcessor eventProcessor) : UserEventConsumerBase(channel, eventProcessor) { }