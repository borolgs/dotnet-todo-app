
using System.Security.Claims;
using System.Threading.Channels;
using App.Db;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace App;

public class Todo {
  public string? UserId { get; set; }
  public int Id { get; set; }
  public string? Name { get; set; }
  public bool IsComplete { get; set; }
}

public class CreateTodoIn {
  public required string Name { get; set; }
}

public class CreateTodoInValidator : AbstractValidator<CreateTodoIn> {
  public CreateTodoInValidator() {
    RuleFor(s => s.Name).NotNull().MinimumLength(3);
  }
}


public static class Todos {

  public static void AddTodoServices(this IServiceCollection services) {
    services.AddHostedService<TodoUserEventConsumer>();
    services.AddSingleton<TodoUserEventsProcessor>();
  }

  public static void AddTodosEndpoints(this WebApplication app) {

    var router = app.MapGroup("/")
    .RequireAuthorization()
    .AddFluentValidationAutoValidation()
    .WithOpenApi();

    router.MapGet("/api/v1/todos", GetAllTodos);
    router.MapGet("/api/v1/todos/complete", GetCompleteTodos);
    router.MapGet("/api/v1/todos/{id}", GetTodo);
    router.MapPost("/api/v1/todos", CreateTodo);
    router.MapPut("/api/v1/todos/{id}", UpdateTodo);
    router.MapDelete("/api/v1/todos/{id}", DeleteTodo);
  }

  public static async Task<Results<Ok<Todo[]>, NotFound, BadRequest>> GetAllTodos(DbCtx db, HttpContext context) {
    return TypedResults.Ok(await db.Todos.ToArrayAsync());
  }


  static async Task<IResult> GetCompleteTodos(DbCtx db) {
    return TypedResults.Ok(await db.Todos.Where(t => t.IsComplete).ToListAsync());
  }

  static async Task<IResult> GetTodo(int id, DbCtx db) {
    return await db.Todos.FindAsync(id)
        is Todo todo
            ? TypedResults.Ok(todo)
            : TypedResults.NotFound();
  }

  static async Task<Results<Created<Todo>, NotFound, BadRequest>> CreateTodo(CreateTodoIn todoIn, ClaimsPrincipal user, DbCtx db) {
    var userId = user.FindFirst(ClaimTypes.NameIdentifier)!.Value;

    var todo = new Todo {
      UserId = userId,
      Name = todoIn.Name
    };
    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    return TypedResults.Created($"/api/v1/todos/{todo.Id}", todo);
  }

  static async Task<IResult> UpdateTodo(int id, Todo inputTodo, DbCtx db) {
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return TypedResults.NotFound();

    todo.Name = inputTodo.Name;
    todo.IsComplete = inputTodo.IsComplete;

    await db.SaveChangesAsync();

    return TypedResults.NoContent();
  }

  static async Task<IResult> DeleteTodo(int id, DbCtx db) {
    if (await db.Todos.FindAsync(id) is Todo todo) {
      db.Todos.Remove(todo);
      await db.SaveChangesAsync();
      return TypedResults.NoContent();
    }

    return TypedResults.NotFound();
  }
}

class TodoUserEventsProcessor : IUserEventProcessor {
  private readonly IServiceProvider serviceProvider;
  private readonly ILogger<TodoUserEventsProcessor> logger;
  public TodoUserEventsProcessor(IServiceProvider serviceProvider, ILogger<TodoUserEventsProcessor> logger) {
    this.serviceProvider = serviceProvider;
    this.logger = logger;
  }

  public Task ProcessAsync(UserEvent userEvent, CancellationToken stoppingToken) {
    using var scope = serviceProvider.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<DbCtx>();

    logger.LogInformation($"Consume {userEvent}");
    return Task.CompletedTask;
  }
}

class TodoUserEventConsumer(Channel<UserEvent> channel, TodoUserEventsProcessor eventProcessor) : UserEventConsumerBase(channel, eventProcessor) { }