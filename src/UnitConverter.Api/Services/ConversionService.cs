using UnitConverter.Api.Models;

namespace UnitConverter.Api.Services;

public interface IConversionService
{
    ConversionResult Convert(ConversionRequest request);
    IReadOnlyList<CategoryInfo> GetCategories();
}

public sealed class ConversionService : IConversionService
{
    // Each unit is defined as an affine map to a base unit:
    //   base = (value + Offset) * Multiplier
    // This handles both linear units (offset = 0) and temperature (e.g. Celsius → Kelvin).
    private sealed record UnitDefinition(string Symbol, string Name, double Multiplier, double Offset = 0);
    private sealed record Category(string Id, string Name, UnitDefinition[] Units);

    private readonly Category[] _categories =
    [
        // Base unit: metre
        new("length", "Length",
        [
            new("m",  "Metre",      1.0),
            new("km", "Kilometre",  1000.0),
            new("cm", "Centimetre", 0.01),
            new("mm", "Millimetre", 0.001),
            new("in", "Inch",       0.0254),
            new("ft", "Foot",       0.3048),
            new("yd", "Yard",       0.9144),
            new("mi", "Mile",       1609.344),
        ]),

        // Base unit: Kelvin
        // Formula: K = (value + Offset) * Multiplier
        new("temperature", "Temperature",
        [
            new("K", "Kelvin",     1.0,        0.0),
            new("C", "Celsius",    1.0,        273.15),
            new("F", "Fahrenheit", 5.0 / 9.0,  459.67),
        ]),

        // Base unit: kilogram
        new("weight", "Weight / Mass",
        [
            new("kg", "Kilogram",     1.0),
            new("g",  "Gram",         0.001),
            new("mg", "Milligram",    1e-6),
            new("t",  "Metric Tonne", 1000.0),
            new("lb", "Pound",        0.45359237),
            new("oz", "Ounce",        0.028349523125),
        ]),
    ];

    public ConversionResult Convert(ConversionRequest request)
    {
        // Find the category that contains both units
        var category = _categories.FirstOrDefault(c =>
            c.Units.Any(u => u.Symbol.Equals(request.FromUnit, StringComparison.OrdinalIgnoreCase)) &&
            c.Units.Any(u => u.Symbol.Equals(request.ToUnit, StringComparison.OrdinalIgnoreCase)));

        if (category is null)
            throw new ArgumentException(
                $"Could not find a category containing both '{request.FromUnit}' and '{request.ToUnit}'. " +
                "Units may be unknown or belong to different categories.");

        var fromUnit = category.Units.First(u => u.Symbol.Equals(request.FromUnit, StringComparison.OrdinalIgnoreCase));
        var toUnit   = category.Units.First(u => u.Symbol.Equals(request.ToUnit,   StringComparison.OrdinalIgnoreCase));

        // Convert: source → base → target
        var baseValue   = (request.Value + fromUnit.Offset) * fromUnit.Multiplier;
        var outputValue = baseValue / toUnit.Multiplier - toUnit.Offset;

        return new ConversionResult
        {
            InputValue   = request.Value,
            FromUnit     = fromUnit.Symbol,
            FromUnitName = fromUnit.Name,
            OutputValue  = outputValue,
            ToUnit       = toUnit.Symbol,
            ToUnitName   = toUnit.Name,
            Category     = category.Name,
        };
    }

    public IReadOnlyList<CategoryInfo> GetCategories() =>
        _categories.Select(c => new CategoryInfo
        {
            Id    = c.Id,
            Name  = c.Name,
            Units = c.Units.Select(u => new UnitInfo { Symbol = u.Symbol, Name = u.Name }).ToList(),
        }).ToList();
}
