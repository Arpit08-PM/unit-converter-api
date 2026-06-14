using Microsoft.AspNetCore.Mvc;
using UnitConverter.Api.Models;
using UnitConverter.Api.Services;

namespace UnitConverter.Api.Controllers;

[ApiController]
[Route("api")]
[Produces("application/json")]
public sealed class ConversionsController : ControllerBase
{
    private readonly IConversionService _conversionService;
    private readonly ILogger<ConversionsController> _logger;

    public ConversionsController(IConversionService conversionService, ILogger<ConversionsController> logger)
    {
        _conversionService = conversionService;
        _logger = logger;
    }

    [HttpPost("convert")]
    public IActionResult Convert([FromBody] ConversionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FromUnit) || string.IsNullOrWhiteSpace(request.ToUnit))
        {
            _logger.LogWarning("Convert called with missing unit parameters");
            return BadRequest(new { error = "FromUnit and ToUnit are required." });
        }

        try
        {
            _logger.LogInformation("Converting {Value} from {FromUnit} to {ToUnit}", request.Value, request.FromUnit, request.ToUnit);
            var result = _conversionService.Convert(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Conversion failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("categories")]
    public IActionResult GetCategories()
    {
        _logger.LogInformation("Fetching all categories");
        return Ok(_conversionService.GetCategories());
    }
}