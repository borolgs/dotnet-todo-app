using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit.Abstractions;

namespace Tests;


public class TodoApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>> {
  private readonly ITestOutputHelper output;

  private readonly HttpClient _httpClient;

  public TodoApiIntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper output) {

    this.output = output;

    _httpClient = factory.CreateClient(new WebApplicationFactoryClientOptions {
      BaseAddress = new Uri("http://localhost:5000")
    });
  }

  [Fact]
  public async Task GetTodoItemsAuthRequired() {

    var response = await _httpClient.GetAsync("/api/v1/todos");

    var responseStatusCode = response.StatusCode;

    Assert.Equal(HttpStatusCode.Unauthorized, responseStatusCode);
  }
}