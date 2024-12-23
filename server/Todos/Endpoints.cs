
using App.Db;
using App.Shared;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace App.Todos;


public static partial class Todos {

  public static void AddTodoServices(this IServiceCollection services) {
    services.AddChannelBasedConsumer<UserEvent, TodoUserEventConsumer>();

    services.AddScoped(provider => {
      var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
      var dbContext = provider.GetRequiredService<DbCtx>();
      return new Ctx(httpContextAccessor.HttpContext!, dbContext);
    });
  }

  public static void AddTodosEndpoints(this WebApplication app) {

    var router = app.MapGroup("/")
    .RequireAuthorization()
    .AddFluentValidationAutoValidation()
    .WithOpenApi().WithTags(["Todos"]);

    router.MapPost("/api/v1/todos", CreateTodo);
    router.MapGet("/api/v1/todos/{id}", GetTodo);
    router.MapPut("/api/v1/todos/{id}", UpdateTodo);
    router.MapDelete("/api/v1/todos/{id}", DeleteTodo);
    router.MapGet("/api/v1/todos", GetAllTodos);
    router.MapGet("/api/v1/todos/complete", GetCompleteTodos);
  }

}