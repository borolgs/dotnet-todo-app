using System.Threading.Channels;
using App.Db;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace App;

public class User : IdentityUser {

}

public abstract record UserEvent;
public record UserCreated(User user) : UserEvent;

public class ApplicationUserManager : UserManager<User> {
  private readonly Channel<UserEvent> _channel;
  public ApplicationUserManager(IUserStore<User> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<User> passwordHasher, IEnumerable<IUserValidator<User>> userValidators, IEnumerable<IPasswordValidator<User>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<User>> logger, Channel<UserEvent> channel) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger) {
    _channel = channel;
  }

  public override async Task<IdentityResult> CreateAsync(User user, string password) {
    var result = await base.CreateAsync(user, password);

    if (result.Succeeded) {
      await _channel.Writer.WriteAsync(new UserCreated(user));
      Logger.LogInformation("User Created");
    }

    return result;
  }
}

public static class AuthExtensions {
  public static void AddAuthServices(this IServiceCollection services, IWebHostEnvironment environment) {
    services.AddSingleton(Channel.CreateUnbounded<UserEvent>());
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

    services.ConfigureApplicationCookie(options => {
      options.Events.OnRedirectToLogin = context => {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
      };
    });
  }

  public static void AddAuthEndpoints(this WebApplication app) {
    app.MapIdentityApi<User>().WithTags(["Auth"]);
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