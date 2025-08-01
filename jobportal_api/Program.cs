using jobportal_api;
using jobportal_api.Services;
//needed for JWT authentication
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
var connectionString = builder.Configuration.GetConnectionString("JobPortalDb");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

//JWT configuration
//var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKey";
//var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "http://localhost:20099/api/";
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings["Key"];
var jwtIssuer = jwtSettings["Issuer"];



builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});



// Use SQL Server for session persistence
//builder.Services.AddDistributedSqlServerCache(options =>
//{
//    options.ConnectionString = connectionString;
//    options.SchemaName = "dbo";
//    options.TableName = "Sessions"; // This table will store session data
//});





// Session middleware
//builder.Services.AddSession(options =>
//{
//    options.IdleTimeout = TimeSpan.FromDays(1); // Session timeout
//    options.Cookie.HttpOnly = true;
//    options.Cookie.IsEssential = true;
//    options.Cookie.SameSite = SameSiteMode.Lax;
//    options.Cookie.Name = "JobPortalSession";
//});

// CORS policy

builder.Services.AddScoped<JWTService>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("AllowReactApp");
//app.UseSession(); //  Must be before `UseAuthorization`
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
