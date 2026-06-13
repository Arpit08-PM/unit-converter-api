namespace UnitConverter.Api.Exceptions;

/// <summary>
/// Thrown when a requested unit symbol is not recognised within a given category,
/// or when the two units belong to different categories.
/// </summary>
public sealed class UnitNotFoundException : Exception
{
    public string Symbol { get; }

    public UnitNotFoundException(string symbol)
        : base($"Unit '{symbol}' is not recognised. Use GET /api/categories to list all available units.")
    {
        Symbol = symbol;
    }
}

/// <summary>
/// Thrown when a conversion is attempted between units that belong to different categories
/// (e.g. trying to convert meters to kilograms).
/// </summary>
public sealed class IncompatibleUnitsException : Exception
{
    public string FromUnit { get; }
    public string ToUnit { get; }

    public IncompatibleUnitsException(string fromUnit, string toUnit)
        : base($"Cannot convert from '{fromUnit}' to '{toUnit}': units belong to different categories.")
    {
        FromUnit = fromUnit;
        ToUnit = toUnit;
    }
}
