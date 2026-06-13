using FluentAssertions;
using UnitConverter.Api.Exceptions;
using UnitConverter.Api.Models;
using UnitConverter.Api.Services;
using Xunit;

namespace UnitConverter.Tests.Services;

public sealed class ConversionServiceTests
{
    private readonly ConversionService _sut;

    public ConversionServiceTests()
    {
        _sut = new ConversionService(new InMemoryUnitRegistry());
    }

    // ── Length ───────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(1.0,  "m",  "cm",  100.0)]
    [InlineData(1.0,  "km", "m",   1000.0)]
    [InlineData(1.0,  "m",  "ft",  3.280_839_895)]
    [InlineData(1.0,  "mi", "km",  1.609_344)]
    [InlineData(1.0,  "in", "cm",  2.54)]
    public void Convert_Length_ReturnsCorrectResult(double input, string from, string to, double expected)
    {
        var result = _sut.Convert(new ConversionRequest { Value = input, FromUnit = from, ToUnit = to });

        result.OutputValue.Should().BeApproximately(expected, precision: 1e-6);
        result.Category.Should().Be("Length");
    }

    // ── Temperature ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0.0,    "C", "F",  32.0)]
    [InlineData(100.0,  "C", "F",  212.0)]
    [InlineData(0.0,    "C", "K",  273.15)]
    [InlineData(-40.0,  "C", "F",  -40.0)]   // The one temperature where C == F
    [InlineData(212.0,  "F", "C",  100.0)]
    [InlineData(32.0,   "F", "K",  273.15)]
    public void Convert_Temperature_ReturnsCorrectResult(double input, string from, string to, double expected)
    {
        var result = _sut.Convert(new ConversionRequest { Value = input, FromUnit = from, ToUnit = to });

        result.OutputValue.Should().BeApproximately(expected, precision: 1e-6);
    }

    // ── Weight ───────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(1.0,   "kg", "lb",  2.204_622_62)]
    [InlineData(1.0,   "lb", "kg",  0.453_592_37)]
    [InlineData(1000.0,"g",  "kg",  1.0)]
    [InlineData(1.0,   "t",  "kg",  1000.0)]
    public void Convert_Weight_ReturnsCorrectResult(double input, string from, string to, double expected)
    {
        var result = _sut.Convert(new ConversionRequest { Value = input, FromUnit = from, ToUnit = to });

        result.OutputValue.Should().BeApproximately(expected, precision: 1e-6);
    }

    // ── Identity conversions ─────────────────────────────────────────────────

    [Theory]
    [InlineData(42.0, "m")]
    [InlineData(0.0,  "C")]
    [InlineData(1.0,  "kg")]
    public void Convert_SameUnit_ReturnsSameValue(double value, string unit)
    {
        var result = _sut.Convert(new ConversionRequest { Value = value, FromUnit = unit, ToUnit = unit });

        result.OutputValue.Should().BeApproximately(value, precision: 1e-10);
    }

    // ── Error cases ──────────────────────────────────────────────────────────

    [Fact]
    public void Convert_UnknownFromUnit_ThrowsUnitNotFoundException()
    {
        var act = () => _sut.Convert(new ConversionRequest { Value = 1, FromUnit = "UNKNOWN", ToUnit = "m" });

        act.Should().Throw<UnitNotFoundException>().Which.Symbol.Should().Be("UNKNOWN");
    }

    [Fact]
    public void Convert_UnknownToUnit_ThrowsUnitNotFoundException()
    {
        var act = () => _sut.Convert(new ConversionRequest { Value = 1, FromUnit = "m", ToUnit = "UNKNOWN" });

        act.Should().Throw<UnitNotFoundException>();
    }

    [Fact]
    public void Convert_IncompatibleUnits_ThrowsIncompatibleUnitsException()
    {
        var act = () => _sut.Convert(new ConversionRequest { Value = 1, FromUnit = "m", ToUnit = "kg" });

        act.Should().Throw<IncompatibleUnitsException>();
    }

    // ── Catalogue queries ────────────────────────────────────────────────────

    [Fact]
    public void GetCategories_ReturnsAtLeastThreeCategories()
    {
        var categories = _sut.GetCategories();

        categories.Should().HaveCountGreaterThanOrEqualTo(3);
        categories.Select(c => c.Id).Should().Contain(["length", "temperature", "weight"]);
    }

    [Fact]
    public void GetUnitsForCategory_KnownCategory_ReturnsUnits()
    {
        var units = _sut.GetUnitsForCategory("length");

        units.Should().NotBeEmpty();
        units.Should().Contain(u => u.Symbol == "m");
        units.Should().Contain(u => u.Symbol == "ft");
    }

    [Fact]
    public void GetUnitsForCategory_UnknownCategory_ReturnsEmptyList()
    {
        var units = _sut.GetUnitsForCategory("nonexistent");

        units.Should().BeEmpty();
    }
}
