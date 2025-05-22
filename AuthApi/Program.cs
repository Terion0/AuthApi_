using AuthApi.DbsContext;
using AuthApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Parte del DBCONTEXT
var dbHost = builder.Configuration["DbSettings:Host"];
var dbPort = builder.Configuration["DbSettings:Port"];
var dbUsername = builder.Configuration["DbSettings:Username"];
var dbPassword = builder.Configuration["DbSettings:Password"];
var dbName = builder.Configuration["DbSettings:Database"];
var connectionString = $"Host={dbHost};Username={dbUsername};Password={dbPassword};Database={dbName};Port={dbPort}";
builder.Services.AddDbContext<AuDbContext>(opt =>
    opt.UseNpgsql(connectionString));


//Parte del JWT
builder.Services.AddScoped<JWTService>();
//Parte del Email
builder.Services.AddScoped<EmailService>();
//Parte de encriptación
builder.Services.AddScoped<EncryptService>();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuDbContext>();
    dbContext.Database.Migrate();
}
app.UseSwagger();
app.UseSwaggerUI();


app.UseAuthentication();  
app.UseAuthorization();
app.MapControllers();

app.Run();
