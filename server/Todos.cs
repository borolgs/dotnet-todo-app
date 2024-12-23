
using App.Db;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace App;

public class Todo {
  public int Id { get; set; }
  public string? Name { get; set; }
  public bool IsComplete { get; set; }
}

public static class Todos {
  public static void RegisterTodosRoutes(this WebApplication app) {

    var router = app.MapGroup("/").RequireAuthorization().WithOpenApi();

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

  static async Task<Results<Created<Todo>, NotFound, BadRequest>> CreateTodo(Todo todo, DbCtx db) {
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


