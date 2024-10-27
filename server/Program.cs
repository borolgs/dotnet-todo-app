using App;
using App.Db;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextPool<DbCtx>((opt) => opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

if (builder.Environment.IsDevelopment()) {
  builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-8.0
builder.Logging.AddJsonConsole();

// https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-8.0
builder.Services.AddHealthChecks().AddDbContextCheck<DbCtx>();

// https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-8.0
builder.Services.AddRateLimiter(o => o
    .AddFixedWindowLimiter(policyName: "fixed", options => {
      // configuration
    }));

// https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-8.0
builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<DbCtx>();

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-logging/?view=aspnetcore-8.0#http-logging-options
builder.Services.AddHttpLogging(options => { });

// https://learn.microsoft.com/en-us/aspnet/core/log-mon/metrics/metrics?view=aspnetcore-8.0
builder.Services.AddOpenTelemetry().WithMetrics(
  builder => {
    builder.AddPrometheusExporter();
    builder.AddMeter("Microsoft.AspNetCore.Hosting",
                     "Microsoft.AspNetCore.Server.Kestrel");
    builder.AddView("http.server.request.duration",
        new ExplicitBucketHistogramConfiguration {
          Boundaries = [ 0, 0.005, 0.01, 0.025, 0.05,
                       0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10 ]
        });
  }
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


app.UseExceptionHandler(exceptionHandlerApp =>
  exceptionHandlerApp.Run(async httpContext => {
    // TODO: handle error
    // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/handle-errors?view=aspnetcore-8.0
    await Results.Problem().ExecuteAsync(httpContext);
  }));

app.UseHttpLogging();

app.MapPrometheusScrapingEndpoint();

app.UseSwagger();
app.UseSwaggerUI(config => {
  config.DocumentTitle = "SoftwareMarketAPI";
});

app.UseRateLimiter();

app.MapGet("/health", async (HealthCheckService healthCheckService) => {
  var report = await healthCheckService.CheckHealthAsync();
  if (report.Status == HealthStatus.Healthy) {
    return Results.Ok(report);
  }
  return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
}).ExcludeFromDescription();
app.MapGet("/ready", () => Results.Ok()).ExcludeFromDescription();

app.MapIdentityApi<IdentityUser>().WithTags(["Auth"]);

app.RegisterTodosRoutes();


app.Run();