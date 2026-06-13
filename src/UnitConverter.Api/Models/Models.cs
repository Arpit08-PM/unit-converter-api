namespace UnitConverter.Api.Models;

public sealed record ConversionRequest
{
    public double Value { get; init; }
    public required string FromUnit { get; init; }
    public required string ToUnit { get; init; }
}

public sealed record ConversionResult
{
    public double InputValue { get; init; }
    public required string FromUnit { get; init; }
    public required string FromUnitName { get; init; }
    public double OutputValue { get; init; }
    public required string ToUnit { get; init; }
    public required string ToUnitName { get; init; }
    public required string Category { get; init; }
}

public sealed record UnitInfo
{
    public required string Symbol { get; init; }
    public required string Name { get; init; }
}

public sealed record CategoryInfo
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required IReadOnlyList<UnitInfo> Units { get; init; }
}
