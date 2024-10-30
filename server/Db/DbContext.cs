using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace App.Db;

public class DbCtx(DbContextOptions<DbCtx> options) : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options) {

  public DbSet<Todo> Todos => Set<Todo>();
  public DbSet<Editor> Editors => Set<Editor>();

  protected override void OnModelCreating(ModelBuilder modelBuilder) {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Editor>()
        .HasOne(e => e.User)
        .WithOne()
        .HasForeignKey<Editor>(e => e.Id)
        .IsRequired();

    modelBuilder.Entity<Todo>()
        .HasOne(t => t.Editor)
        .WithMany(e => e.Todos)
        .HasForeignKey(t => t.EditorId)
        .IsRequired();
  }
}