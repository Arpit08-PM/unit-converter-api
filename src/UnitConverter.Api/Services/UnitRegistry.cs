using UnitConverter.Api.Models;

namespace UnitConverter.Api.Services;

/// <summary>
/// Holds all supported conversion categories and their unit definitions.
///
/// Design note:
///   Units are stored as an affine map to a canonical base unit for each category:
///       base_value = (input_value + ToBaseOffset) * ToBaseMultiplier
///   This representation handles both linear (e.g. length) and offset-linear
///   (e.g. temperature) unit families with a single formula, keeping the data
///   model simple and extensible.
///
///   When hundreds of units must be supported in the future, the same interface
///   is satisfied by swapping this in-memory registry for a database-backed
///   implementation of IUnitRegistry without touching the conversion logic.
/// </summary>
public interface IUnitRegistry
{
    IReadOnlyList<ConversionCategory> GetAllCategories();
    ConversionCategory? FindCategoryByUnit(string symbol);
    UnitDefinition? FindUnit(string symbol);
}

public sealed class InMemoryUnitRegistry : IUnitRegistry
{
    private readonly IReadOnlyList<ConversionCategory> _categories;

    // Index from lower-case unit symbol → category for O(1) look-ups.
    private readonly Dictionary<string, ConversionCategory> _symbolToCategory;
    private readonly Dictionary<string, UnitDefinition> _symbolToUnit;

    public InMemoryUnitRegistry()
    {
        _categories = BuildCategories();

        _symbolToCategory = new Dictionary<string, ConversionCategory>(StringComparer.OrdinalIgnoreCase);
        _symbolToUnit = new Dictionary<string, UnitDefinition>(StringComparer.OrdinalIgnoreCase);

        foreach (var category in _categories)
        {
            foreach (var unit in category.Units)
            {
                _symbolToCategory[unit.Symbol] = category;
                _symbolToUnit[unit.Symbol] = unit;
            }
        }
    }

    public IReadOnlyList<ConversionCategory> GetAllCategories() => _categories;

    public ConversionCategory? FindCategoryByUnit(string symbol) =>
        _symbolToCategory.TryGetValue(symbol, out var cat) ? cat : null;

    public UnitDefinition? FindUnit(string symbol) =>
        _symbolToUnit.TryGetValue(symbol, out var unit) ? unit : null;

    // -----------------------------------------------------------------------
    // Unit data
    // Base units are denoted with ToBaseMultiplier = 1 and ToBaseOffset = 0.
    // For affine conversions: base = (value + offset) * multiplier
    // -----------------------------------------------------------------------
    private static IReadOnlyList<ConversionCategory> BuildCategories() =>
    [
        // ── LENGTH (base: metre) ──────────────────────────────────────────
        new ConversionCategory
        {
            Id = "length",
            Name = "Length",
            Description = "Units of linear distance.",
            Units =
            [
                new() { Symbol = "m",   Name = "Metre",      ToBaseMultiplier = 1.0 },
                new() { Symbol = "km",  Name = "Kilometre",  ToBaseMultiplier = 1_000.0 },
                new() { Symbol = "cm",  Name = "Centimetre", ToBaseMultiplier = 0.01 },
                new() { Symbol = "mm",  Name = "Millimetre", ToBaseMultiplier = 0.001 },
                new() { Symbol = "um",  Name = "Micrometre", ToBaseMultiplier = 1e-6 },
                new() { Symbol = "nm",  Name = "Nanometre",  ToBaseMultiplier = 1e-9 },
                new() { Symbol = "in",  Name = "Inch",       ToBaseMultiplier = 0.0254 },
                new() { Symbol = "ft",  Name = "Foot",       ToBaseMultiplier = 0.3048 },
                new() { Symbol = "yd",  Name = "Yard",       ToBaseMultiplier = 0.9144 },
                new() { Symbol = "mi",  Name = "Mile",       ToBaseMultiplier = 1_609.344 },
                new() { Symbol = "nmi", Name = "Nautical Mile", ToBaseMultiplier = 1_852.0 },
                new() { Symbol = "ly",  Name = "Light Year", ToBaseMultiplier = 9.461e15 },
            ]
        },

        // ── TEMPERATURE (base: Kelvin) ────────────────────────────────────
        // Formula: K = (value + ToBaseOffset) * ToBaseMultiplier
        new ConversionCategory
        {
            Id = "temperature",
            Name = "Temperature",
            Description = "Units of thermal energy / temperature.",
            Units =
            [
                new() { Symbol = "K",  Name = "Kelvin",     ToBaseMultiplier = 1.0,          ToBaseOffset = 0.0 },
                new() { Symbol = "C",  Name = "Celsius",    ToBaseMultiplier = 1.0,          ToBaseOffset = 273.15 },
                new() { Symbol = "F",  Name = "Fahrenheit", ToBaseMultiplier = 5.0 / 9.0,   ToBaseOffset = 459.67 },
                new() { Symbol = "R",  Name = "Rankine",    ToBaseMultiplier = 5.0 / 9.0,   ToBaseOffset = 0.0 },
            ]
        },

        // ── WEIGHT / MASS (base: kilogram) ───────────────────────────────
        new ConversionCategory
        {
            Id = "weight",
            Name = "Weight / Mass",
            Description = "Units of mass.",
            Units =
            [
                new() { Symbol = "kg",  Name = "Kilogram",       ToBaseMultiplier = 1.0 },
                new() { Symbol = "g",   Name = "Gram",           ToBaseMultiplier = 0.001 },
                new() { Symbol = "mg",  Name = "Milligram",      ToBaseMultiplier = 1e-6 },
                new() { Symbol = "t",   Name = "Metric Tonne",   ToBaseMultiplier = 1_000.0 },
                new() { Symbol = "lb",  Name = "Pound",          ToBaseMultiplier = 0.453_592_37 },
                new() { Symbol = "oz",  Name = "Ounce",          ToBaseMultiplier = 0.028_349_523_125 },
                new() { Symbol = "st",  Name = "Stone",          ToBaseMultiplier = 6.350_293_18 },
                new() { Symbol = "ton", Name = "Short Ton (US)", ToBaseMultiplier = 907.184_74 },
                new() { Symbol = "lton",Name = "Long Ton (UK)",  ToBaseMultiplier = 1_016.046_909 },
            ]
        },

        // ── AREA (base: square metre) ────────────────────────────────────
        new ConversionCategory
        {
            Id = "area",
            Name = "Area",
            Description = "Units of two-dimensional extent.",
            Units =
            [
                new() { Symbol = "m2",   Name = "Square Metre",      ToBaseMultiplier = 1.0 },
                new() { Symbol = "km2",  Name = "Square Kilometre",  ToBaseMultiplier = 1e6 },
                new() { Symbol = "cm2",  Name = "Square Centimetre", ToBaseMultiplier = 1e-4 },
                new() { Symbol = "mm2",  Name = "Square Millimetre", ToBaseMultiplier = 1e-6 },
                new() { Symbol = "ha",   Name = "Hectare",           ToBaseMultiplier = 10_000.0 },
                new() { Symbol = "ac",   Name = "Acre",              ToBaseMultiplier = 4_046.856_422_4 },
                new() { Symbol = "ft2",  Name = "Square Foot",       ToBaseMultiplier = 0.092_903_04 },
                new() { Symbol = "in2",  Name = "Square Inch",       ToBaseMultiplier = 6.4516e-4 },
                new() { Symbol = "yd2",  Name = "Square Yard",       ToBaseMultiplier = 0.836_127_36 },
                new() { Symbol = "mi2",  Name = "Square Mile",       ToBaseMultiplier = 2_589_988.110_336 },
            ]
        },

        // ── VOLUME (base: cubic metre) ───────────────────────────────────
        new ConversionCategory
        {
            Id = "volume",
            Name = "Volume",
            Description = "Units of three-dimensional capacity.",
            Units =
            [
                new() { Symbol = "m3",   Name = "Cubic Metre",     ToBaseMultiplier = 1.0 },
                new() { Symbol = "L",    Name = "Litre",           ToBaseMultiplier = 0.001 },
                new() { Symbol = "mL",   Name = "Millilitre",      ToBaseMultiplier = 1e-6 },
                new() { Symbol = "cm3",  Name = "Cubic Centimetre",ToBaseMultiplier = 1e-6 },
                new() { Symbol = "ft3",  Name = "Cubic Foot",      ToBaseMultiplier = 0.028_316_846_592 },
                new() { Symbol = "in3",  Name = "Cubic Inch",      ToBaseMultiplier = 1.638_706_4e-5 },
                new() { Symbol = "gal",  Name = "US Gallon",       ToBaseMultiplier = 0.003_785_411_784 },
                new() { Symbol = "qt",   Name = "US Quart",        ToBaseMultiplier = 9.463_529_46e-4 },
                new() { Symbol = "pt",   Name = "US Pint",         ToBaseMultiplier = 4.731_764_73e-4 },
                new() { Symbol = "floz", Name = "US Fluid Ounce",  ToBaseMultiplier = 2.957_352_956_25e-5 },
                new() { Symbol = "ukgal",Name = "Imperial Gallon", ToBaseMultiplier = 0.004_546_09 },
            ]
        },

        // ── SPEED (base: metres per second) ─────────────────────────────
        new ConversionCategory
        {
            Id = "speed",
            Name = "Speed",
            Description = "Units of velocity.",
            Units =
            [
                new() { Symbol = "mps",  Name = "Metres per Second",     ToBaseMultiplier = 1.0 },
                new() { Symbol = "kph",  Name = "Kilometres per Hour",   ToBaseMultiplier = 1.0 / 3.6 },
                new() { Symbol = "mph",  Name = "Miles per Hour",        ToBaseMultiplier = 0.447_04 },
                new() { Symbol = "knot", Name = "Knot",                  ToBaseMultiplier = 0.514_444 },
                new() { Symbol = "fps",  Name = "Feet per Second",       ToBaseMultiplier = 0.3048 },
                new() { Symbol = "mach", Name = "Mach (sea level, 15°C)",ToBaseMultiplier = 340.29 },
            ]
        },

        // ── PRESSURE (base: Pascal) ──────────────────────────────────────
        new ConversionCategory
        {
            Id = "pressure",
            Name = "Pressure",
            Description = "Units of force per unit area.",
            Units =
            [
                new() { Symbol = "Pa",   Name = "Pascal",              ToBaseMultiplier = 1.0 },
                new() { Symbol = "kPa",  Name = "Kilopascal",          ToBaseMultiplier = 1_000.0 },
                new() { Symbol = "MPa",  Name = "Megapascal",          ToBaseMultiplier = 1e6 },
                new() { Symbol = "bar",  Name = "Bar",                 ToBaseMultiplier = 100_000.0 },
                new() { Symbol = "mbar", Name = "Millibar",            ToBaseMultiplier = 100.0 },
                new() { Symbol = "atm",  Name = "Atmosphere",          ToBaseMultiplier = 101_325.0 },
                new() { Symbol = "psi",  Name = "Pounds per Sq Inch",  ToBaseMultiplier = 6_894.757_293_168 },
                new() { Symbol = "mmHg", Name = "Millimetre of Mercury",ToBaseMultiplier = 133.322_387_415 },
                new() { Symbol = "inHg", Name = "Inch of Mercury",     ToBaseMultiplier = 3_386.389 },
            ]
        },

        // ── ENERGY (base: Joule) ─────────────────────────────────────────
        new ConversionCategory
        {
            Id = "energy",
            Name = "Energy",
            Description = "Units of work and heat.",
            Units =
            [
                new() { Symbol = "J",    Name = "Joule",               ToBaseMultiplier = 1.0 },
                new() { Symbol = "kJ",   Name = "Kilojoule",           ToBaseMultiplier = 1_000.0 },
                new() { Symbol = "MJ",   Name = "Megajoule",           ToBaseMultiplier = 1e6 },
                new() { Symbol = "cal",  Name = "Calorie (thermochem)",ToBaseMultiplier = 4.184 },
                new() { Symbol = "kcal", Name = "Kilocalorie",         ToBaseMultiplier = 4_184.0 },
                new() { Symbol = "Wh",   Name = "Watt-hour",           ToBaseMultiplier = 3_600.0 },
                new() { Symbol = "kWh",  Name = "Kilowatt-hour",       ToBaseMultiplier = 3_600_000.0 },
                new() { Symbol = "BTU",  Name = "British Thermal Unit",ToBaseMultiplier = 1_055.055_852_62 },
                new() { Symbol = "eV",   Name = "Electronvolt",        ToBaseMultiplier = 1.602_176_634e-19 },
            ]
        },

        // ── DATA (base: bit) ─────────────────────────────────────────────
        new ConversionCategory
        {
            Id = "data",
            Name = "Digital Storage",
            Description = "Units of digital information (SI and binary prefixes).",
            Units =
            [
                new() { Symbol = "bit",  Name = "Bit",       ToBaseMultiplier = 1.0 },
                new() { Symbol = "B",    Name = "Byte",      ToBaseMultiplier = 8.0 },
                new() { Symbol = "KB",   Name = "Kilobyte",  ToBaseMultiplier = 8_000.0 },
                new() { Symbol = "MB",   Name = "Megabyte",  ToBaseMultiplier = 8e6 },
                new() { Symbol = "GB",   Name = "Gigabyte",  ToBaseMultiplier = 8e9 },
                new() { Symbol = "TB",   Name = "Terabyte",  ToBaseMultiplier = 8e12 },
                new() { Symbol = "PB",   Name = "Petabyte",  ToBaseMultiplier = 8e15 },
                new() { Symbol = "KiB",  Name = "Kibibyte",  ToBaseMultiplier = 8.0 * 1024 },
                new() { Symbol = "MiB",  Name = "Mebibyte",  ToBaseMultiplier = 8.0 * 1024 * 1024 },
                new() { Symbol = "GiB",  Name = "Gibibyte",  ToBaseMultiplier = 8.0 * 1024 * 1024 * 1024 },
                new() { Symbol = "TiB",  Name = "Tebibyte",  ToBaseMultiplier = 8.0 * 1024 * 1024 * 1024 * 1024 },
            ]
        },
    ];
}
