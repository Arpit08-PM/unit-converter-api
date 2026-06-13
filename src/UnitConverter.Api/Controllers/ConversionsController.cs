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

    public ConversionsController(IConversionService conversionService)
    {
        _conversionService = conversionService;
    }

    /// <summary>Convert a value from one unit to another.</summary>
    [HttpPost("convert")]
    public IActionResult Convert([FromBody] ConversionRequest request)
    {
        try
        {
            var result = _conversionService.Convert(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>List all supported categories and their units.</summary>
    [HttpGet("categories")]
    public IActionResult GetCategories()
    {
        return Ok(_conversionService.GetCategories());
    }
}
