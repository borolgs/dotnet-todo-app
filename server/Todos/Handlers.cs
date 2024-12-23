
using System.ComponentModel.DataAnnotations;
using App.Db;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace App.Todos;

public class Todo {
  public int Id { get; set; }
  public Guid EditorId { get; set; }
  public Editor Editor { get; set; } = null!;
  public string? Name { get; set; }
  public bool IsComplete { get; set; }
}

public class Editor {
  public Guid Id { get; set; }
  public User User { get; set; } = null!;
  public List<Todo> Todos { get; set; } = new();
}

public class CreateTodoIn {
  [Required]
  public required string Name { get; set; }
}

public class CreateTodoOut {
  public int Id { get; set; }
  public Guid EditorId { get; set; }
  public string? Name { get; set; }
  public bool IsComplete { get; set; }
}

public class CreateTodoInValidator : AbstractValidator<CreateTodoIn> {
  public CreateTodoInValidator() {
    RuleFor(s => s.Name).NotNull().MinimumLength(3);
  }
}

public static partial class Todos {
  static async Task<Results<Created<CreateTodoOut>, InternalServerError, NotFound, BadRequest>> CreateTodo
      (CreateTodoIn todoIn, Ctx ctx, DbCtx db) {
    var editor = await ctx.CurrentEditor();

    var todo = new Todo {
      EditorId = editor.Id,
      Name = todoIn.Name
    };

    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    return TypedResults.Created($"/api/v1/todos/{todo.Id}", new CreateTodoOut {
      Id = todo.Id,
      EditorId = editor.Id,
      Name = todo.Name,
      IsComplete = todo.IsComplete
    });
  }

  static async Task<Results<Ok<Todo[]>, NotFound, BadRequest>> GetAllTodos(DbCtx db, HttpContext context) {
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