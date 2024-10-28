using App.Db;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace App;

public class User : IdentityUser {

}

public class ApplicationUserManager(IUserStore<User> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<User> passwordHasher, IEnumerable<IUserValidator<User>> userValidators, IEnumerable<IPasswordValidator<User>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<User>> logger) : UserManager<User>(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger) {
  public override async Task<IdentityResult> CreateAsync(User user, string password) {
    var result = await base.CreateAsync(user, password);

    if (result.Succeeded) {
      Logger.LogInformation("User Created");
    }

    return result;
  }
}

public static class AuthExtensions {
  public static void AddAuthServices(this IServiceCollection services, IWebHostEnvironment environment) {
    // https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-8.0
    // https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-8.0&tabs=net-cli#examine-register
    // https://learn.microsoft.com/en-us/aspnet/core/security/authentication/customize-identity-model?view=aspnetcore-8.0#change-the-primary-key-type
    services.AddAuthorization();
    services
        .AddIdentity<User, IdentityRole>(options => {
          if (environment.IsDevelopment()) {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 4;
            options.Password.RequiredUniqueChars = 1;
          }

          options.SignIn.RequireConfirmedEmail = false;
          options.SignIn.RequireConfirmedPhoneNumber = false;
        })
        .AddUserManager<ApplicationUserManager>()
        .AddApiEndpoints()
        .AddEntityFrameworkStores<DbCtx>();

    // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-logging/?view=aspnetcore-8.0#http-logging-options
    services.AddHttpLogging(options => { });
  }

  public static void AddAuthEndpoints(this WebApplication app) {
    app.MapIdentityApi<User>().WithTags(["Auth"]);
  }
}