using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UnitConverter.Api.Models;
using Xunit;

namespace UnitConverter.Tests.Controllers;

/// <summary>
/// Integration tests that spin up the full ASP.NET Core pipeline via WebApplicationFactory.
/// </summary>
public sealed class ConversionsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public ConversionsControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // ── POST /api/convert ────────────────────────────────────────────────────

    [Fact]
    public async Task Convert_ValidRequest_Returns200WithResult()
    {
        var request = new ConversionRequest { Value = 1000, FromUnit = "m", ToUnit = "km" };

        var response = await _client.PostAsJsonAsync("/api/convert", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ConversionResult>(JsonOptions);
        result.Should().NotBeNull();
        result!.OutputValue.Should().BeApproximately(1.0, precision: 1e-9);
        result.FromUnit.Should().Be("m");
        result.ToUnit.Should().Be("km");
    }

    [Fact]
    public async Task Convert_UnknownUnit_Returns400()
    {
        var request = new ConversionRequest { Value = 1, FromUnit = "BOGUS", ToUnit = "m" };

        var response = await _client.PostAsJsonAsync("/api/convert", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Convert_IncompatibleUnits_Returns400()
    {
        var request = new ConversionRequest { Value = 1, FromUnit = "m", ToUnit = "kg" };

        var response = await _client.PostAsJsonAsync("/api/convert", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Convert_TemperatureCelsiusToFahrenheit_Returns200()
    {
        var request = new ConversionRequest { Value = 0, FromUnit = "C", ToUnit = "F" };

        var response = await _client.PostAsJsonAsync("/api/convert", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ConversionResult>(JsonOptions);
        result!.OutputValue.Should().BeApproximately(32.0, precision: 1e-6);
    }

    // ── GET /api/categories ──────────────────────────────────────────────────

    [Fact]
    public async Task GetCategories_Returns200WithCategories()
    {
        var response = await _client.GetAsync("/api/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var categories = await response.Content.ReadFromJsonAsync<List<CategorySummary>>(JsonOptions);
        categories.Should().NotBeNullOrEmpty();
        categories!.Should().Contain(c => c.Id == "length");
        categories.Should().Contain(c => c.Id == "temperature");
        categories.Should().Contain(c => c.Id == "weight");
    }

    // ── GET /api/categories/{id}/units ───────────────────────────────────────

    [Fact]
    public async Task GetUnits_KnownCategory_Returns200WithUnits()
    {
        var response = await _client.GetAsync("/api/categories/length/units");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var units = await response.Content.ReadFromJsonAsync<List<UnitSummary>>(JsonOptions);
        units.Should().NotBeNullOrEmpty();
        units!.Should().Contain(u => u.Symbol == "m");
    }

    [Fact]
    public async Task GetUnits_UnknownCategory_Returns200WithEmptyList()
    {
        var response = await _client.GetAsync("/api/categories/nonexistent/units");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var units = await response.Content.ReadFromJsonAsync<List<UnitSummary>>(JsonOptions);
        units.Should().BeEmpty();
    }
}
