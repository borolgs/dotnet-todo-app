using System.Security.Claims;
using App.Db;

namespace App.Todos;


public class Ctx(HttpContext httpContext, DbCtx db) {
  public ClaimsPrincipal User => httpContext.User;

  public Guid UserId =>
      new(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? throw new UnauthorizedAccessException("User ID claim is missing."));

  public async Task<Editor> CurrentEditor() {
    return await db.Editors.FindAsync(UserId) ?? throw new UnauthorizedAccessException("Editor not found");
  }
}