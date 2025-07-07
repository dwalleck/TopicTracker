using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using TUnit.Assertions;
using TUnit.Core;

namespace TopicTracker.Api.Tests.Controllers;

public class BasicApiTest
{
    [Test]
    public async Task Api_Should_Respond_To_Basic_Request()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        
        // Act
        var content = new StringContent("Action=Publish", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
        var response = await client.PostAsync("/", content);
        
        // Assert
        await Assert.That(response).IsNotNull();
        await Assert.That((int)response.StatusCode).IsEqualTo(400); // Should fail with missing parameters
    }
}