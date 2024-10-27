using System.Numerics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace App.Db;

public class DbCtx(DbContextOptions<DbCtx> options) : IdentityDbContext<IdentityUser, IdentityRole, string>(options) {
  public DbSet<Todo> Todos => Set<Todo>();
}