using System.Numerics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace App.Db;

public class DbCtx(DbContextOptions<DbCtx> options) : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options) {
  public DbSet<Todo> Todos => Set<Todo>();
}