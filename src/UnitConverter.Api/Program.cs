using UnitConverter.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IConversionService, ConversionService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(o => o.RoutePrefix = string.Empty);
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
