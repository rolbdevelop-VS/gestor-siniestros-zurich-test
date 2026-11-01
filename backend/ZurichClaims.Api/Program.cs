using Microsoft.EntityFrameworkCore;
using ZurichClaims.Api.Data;
using ZurichClaims.Api.Handlers;
using ZurichClaims.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Manejo global de excepciones con ProblemDetails (.NET 8)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Registro de servicios de aplicación
builder.Services.AddScoped<IClaimService, ClaimService>();

// Configuración de la base de datos SQLite con EF Core 8 y ruta determinística contra ContentRoot
var rawConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=claims.db";

var sqliteBuilder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(rawConnectionString);

if (!Path.IsPathRooted(sqliteBuilder.DataSource))
{
    sqliteBuilder.DataSource = Path.Combine(builder.Environment.ContentRootPath, sqliteBuilder.DataSource);
}

builder.Services.AddDbContext<ClaimsDbContext>(options =>
    options.UseSqlite(sqliteBuilder.ConnectionString));

// Configuración estricta de CORS restringida a http://localhost:5173
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Soporte para controladores de API con serialización de enums como string (sin permitir enteros)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter(allowIntegerValues: false));
    });

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();

// Aplicar migraciones pendientes automáticamente al iniciar la aplicación (NO EnsureCreated)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
    dbContext.Database.Migrate();
}

// Configuración del pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("FrontendPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
