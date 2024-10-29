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

          options.SignIn.RequireConfirmedEmail = false;
          options.SignIn.RequireConfirmedPhoneNumber = false;
        })
        .AddUserManager<ApplicationUserManager>()
        .AddApiEndpoints()
        .AddEntityFrameworkStores<DbCtx>();

    services.AddScoped<SignInManager<User>, ApplicationSignInManager>();

    services.ConfigureApplicationCookie(options => {
      options.Events.OnRedirectToLogin = context => {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
      };
    });
  }

  public static void AddAuthEndpoints(this WebApplication app) {
    var authRouter = app.MapGroup("/auth").WithOpenApi().WithTags(["Auth"]);

    authRouter.MapIdentityApi<User>();

    authRouter.MapGet("/logout", async Task<Results<NoContent, BadRequest>> (SignInManager<User> m) => {
      await m.SignOutAsync();
      return TypedResults.NoContent();
    });
  }
}

public class ApplicationUserManager : UserManager<User> {
  private readonly Channel<UserEvent> channel;
  public ApplicationUserManager(IUserStore<User> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<User> passwordHasher, IEnumerable<IUserValidator<User>> userValidators, IEnumerable<IPasswordValidator<User>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<User>> logger, Channel<UserEvent> channel) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger) {
    this.channel = channel;
  }

  public override async Task<IdentityResult> CreateAsync(User user, string password) {
    var result = await base.CreateAsync(user, password);

    if (result.Succeeded) {
      Logger.LogInformation("User Created");
      await channel.Writer.WriteAsync(new UserCreated(user));
    }

    return result;
  }
}

public class ApplicationSignInManager : SignInManager<User> {
  private readonly Channel<UserEvent> channel;

  public ApplicationSignInManager(UserManager<User> userManager, IHttpContextAccessor contextAccessor, IUserClaimsPrincipalFactory<User> claimsFactory, IOptions<IdentityOptions> optionsAccessor, ILogger<SignInManager<User>> logger, IAuthenticationSchemeProvider schemes, IUserConfirmation<User> confirmation, Channel<UserEvent> channel) : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation) {
    this.channel = channel;
  }

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

public interface IUserEventProcessor {
  Task ProcessAsync(UserEvent userEvent, CancellationToken stoppingToken);
}

public abstract class UserEventConsumerBase : BackgroundService {
  private readonly Channel<UserEvent> channel;
  protected readonly IUserEventProcessor eventProcessor;


  protected UserEventConsumerBase(Channel<UserEvent> channel, IUserEventProcessor eventProcessor) {
    this.channel = channel;
    this.eventProcessor = eventProcessor;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
    await ConsumeWithNestedWhileAsync(channel.Reader, stoppingToken);
  }

  private async ValueTask ConsumeWithNestedWhileAsync(
      ChannelReader<UserEvent> reader, CancellationToken stoppingToken) {
    while (await reader.WaitToReadAsync(stoppingToken).ConfigureAwait(false)) {
      while (reader.TryRead(out UserEvent? userEvent)) {
        if (userEvent != null) {
          await ProcessEventAsync(userEvent, stoppingToken);
        }
      }
    }
  }

  private async Task ProcessEventAsync(UserEvent userEvent, CancellationToken stoppingToken) {
    await eventProcessor.ProcessAsync(userEvent, stoppingToken);
  }
}