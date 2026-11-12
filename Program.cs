using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using prakt_8_trpo.Models;
using System.Text;



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
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
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
