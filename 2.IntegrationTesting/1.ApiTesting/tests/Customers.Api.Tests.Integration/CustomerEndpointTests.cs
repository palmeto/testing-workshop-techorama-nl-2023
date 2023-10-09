using System.Net;
using System.Net.Http.Json;
using Customers.Api.Contracts.Requests;
using Customers.Api.Contracts.Responses;
using Customers.Api.Domain;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Customers.Api.Tests.Integration;

public class CustomerEndpointTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<IApiMarker> _waf = new();
    private readonly HttpClient _client;
    private readonly List<Guid> _idsToDelete = new();

    public CustomerEndpointTests()
    {
        _client = _waf.CreateClient();
    }
    
    [Fact]
    public async Task Create_ShouldCreateCustomer_WhenDetailsAreValid()
    {
        // Arrange
        var request = new CustomerRequest
        {
            Email = "nick@chapsas.com",
            FullName = "Nick Chapsas",
            DateOfBirth = new DateTime(1993, 01, 01),
            GitHubUsername = "nickchapsas"
        };
        var expectedResponse = new CustomerResponse
        {
            Email = request.Email,
            FullName = request.FullName,
            DateOfBirth = request.DateOfBirth,
            GitHubUsername = request.GitHubUsername
        };

        // Act
        var response = await _client.PostAsJsonAsync("customers", request);
        var customerResponse = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        _idsToDelete.Add(customerResponse!.Id);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().Be($"http://localhost/customers/{customerResponse!.Id}");
        customerResponse.Should().BeEquivalentTo(expectedResponse, x => x.Excluding(y => y.Id));
        customerResponse.Id.Should().NotBeEmpty();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        foreach (var id in _idsToDelete)
        {
            await _client.DeleteAsync($"customers/{id}");
        }
    }
}
