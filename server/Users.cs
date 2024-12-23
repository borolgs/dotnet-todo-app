using System.Threading.Channels;
using App.Db;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace App;

public class User : IdentityUser<Guid> { }

public abstract record UserEvent;
public record UserCreated(User User) : UserEvent;
public record UserSignedIn(User User) : UserEvent;

public static class AuthExtensions {
  public static void AddAuthServices(this IServiceCollection services, IWebHostEnvironment environment) {
    services.AddSingleton(Channel.CreateUnbounded<UserEvent>());

    // https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-8.0
    // https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-8.0&tabs=net-cli#examine-register
    // https://learn.microsoft.com/en-us/aspnet/core/security/authentication/customize-identity-model?view=aspnetcore-8.0#change-the-primary-key-type
    services.AddAuthorization();
    services
        .AddIdentity<User, IdentityRole<Guid>>(options => {
          if (environment.IsDevelopment()) {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 4;
            options.Password.RequiredUniqueChars = 1;
          }

          // options.SignIn.RequireConfirmedEmail = false;
          options.SignIn.RequireConfirmedPhoneNumber = false;
        })
        .AddUserManager<ApplicationUserManager>()
        .AddApiEndpoints()
        .AddEntityFrameworkStores<DbCtx>();

    services.AddScoped<SignInManager<User>, ApplicationSignInManager>();
    services.AddTransient<IEmailSender<User>, EmailSender>();


    services.ConfigureApplicationCookie(options => {
      options.Events.OnRedirectToLogin = context => {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
      };
    });
  }

  public static void AddAuthEndpoints(this WebApplication app) {
    var authRouter = app.MapGroup("/auth").WithOpenApi().WithTags(["Auth"]);

    authRouter.MapIdentityApi<User>().AddEndpointFilter(async (invocationContext, next) => {
      var result = await next(invocationContext);

      var isMailConfirmRoute = invocationContext.HttpContext.Request.Path.StartsWithSegments("/auth/confirmEmail");
      var isSuccess = invocationContext.HttpContext.Response.StatusCode == 200;

      if (isMailConfirmRoute && isSuccess) {
        return Results.Redirect("/");
      }

      return result;
    });

    authRouter.MapGet("/logout", async Task<Results<NoContent, BadRequest>> (SignInManager<User> m) => {
      await m.SignOutAsync();
      return TypedResults.NoContent();
    });
  }
}

public class ApplicationUserManager(
    IUserStore<User> store,
    IOptions<IdentityOptions> optionsAccessor,
    IPasswordHasher<User> passwordHasher,
    IEnumerable<IUserValidator<User>> userValidators,
    IEnumerable<IPasswordValidator<User>> passwordValidators,
    ILookupNormalizer keyNormalizer,
    IdentityErrorDescriber errors,
    IServiceProvider services,
    ILogger<UserManager<User>> logger,
    Channel<UserEvent> channel
  ) : UserManager<User>(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger) {
  private readonly Channel<UserEvent> channel = channel;

  public override async Task<IdentityResult> CreateAsync(User user, string password) {
    var result = await base.CreateAsync(user, password);

    if (result.Succeeded) {
      Logger.LogInformation("User Created");
      await channel.Writer.WriteAsync(new UserCreated(user));
    }

    return result;
  }
}

public class ApplicationSignInManager(UserManager<User> userManager, IHttpContextAccessor contextAccessor, IUserClaimsPrincipalFactory<User> claimsFactory, IOptions<IdentityOptions> optionsAccessor, ILogger<SignInManager<User>> logger, IAuthenticationSchemeProvider schemes, IUserConfirmation<User> confirmation, Channel<UserEvent> channel) : SignInManager<User>(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation) {
  private readonly Channel<UserEvent> channel = channel;

  public override async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure) {
    var result = await base.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);

    if (result.Succeeded) {
      var user = await UserManager.FindByNameAsync(userName);
      if (user != null) {
        await channel.Writer.WriteAsync(new UserSignedIn(user));
      }
    }

    return result;
  }

}

public class EmailSender(ILogger<EmailSender> logger) : IEmailSender<User> {
  public Task SendConfirmationLinkAsync(User user, string email, string confirmationLink) {
    logger.LogInformation($"SendConfirmationLinkAsync {email} {confirmationLink}");
    return Task.CompletedTask;
  }

  public Task SendPasswordResetCodeAsync(User user, string email, string resetCode) {
    logger.LogInformation($"SendPasswordResetCodeAsync {email} {resetCode}");
    return Task.CompletedTask;
  }

  public Task SendPasswordResetLinkAsync(User user, string email, string resetLink) {
    logger.LogInformation($"SendPasswordResetLinkAsync {email} {resetLink}");
    return Task.CompletedTask;
  }
}