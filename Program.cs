using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using prakt_8_trpo.Models;
using System.Text;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;



var builder = WebApplication.CreateBuilder(args);
var connectionString = "Server=sql.ects;Database=tasksDB;User Id=student_00;password=student_00;TrustServerCertificate=true;";

// Add services to the container.

var key = builder.Configuration [ "Jwt:Key" ]
    ?? throw new Exception("Не задан секретный ключ jwt");

if (Encoding.UTF8.GetByteCount(key) < 32)
    throw new Exception("Ключ JWT слишком короткий");

builder.Services.AddDbContext<TasksDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>( );
builder.Services.AddSingleton<ConcurrentDictionary<string, DateTime>>( );
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "ExampleApi",

            ValidateAudience = true,
            ValidAudience = "ExampleApiClient",

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                if (string.IsNullOrEmpty(jti))
                {
                    context.Fail("В JWT отсутствует jti");
                    return System.Threading.Tasks.Task.CompletedTask;
                }
                var revokedTokens = context.HttpContext.RequestServices.GetRequiredService<ConcurrentDictionary<string, DateTime>>( );
                if (revokedTokens.ContainsKey(jti))
                    context.Fail("JWT отозван");
                return System.Threading.Tasks.Task.CompletedTask;
            }
        };
    }
);


builder.Services.AddControllers( );
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer( );
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference 
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "bearer"
                }
                
            
            },
            Array.Empty<string>()
        }
    });

});


var app = builder.Build( );

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment( ))
{
    app.UseSwagger( );
    app.UseSwaggerUI( );
}

app.UseHttpsRedirection( );

app.UseAuthentication( );

app.UseAuthorization( );

app.MapControllers( );

app.Run( );
