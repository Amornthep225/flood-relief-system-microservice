using System.Text;
using System.Text.Json;
using FloodRelief.Data;
using FloodRelief.Hubs;
using FloodRelief.Services.Auth;
using FloodRelief.Services.Center;
using FloodRelief.Services.Common;
using FloodRelief.Services;
using FloodRelief.Services.Donations;
using FloodRelief.Services.Weather;
using FloodRelief.Services.Notification;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();

builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<CurrentUserService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UsersService>();
builder.Services.AddScoped<StaffsService>();
builder.Services.AddScoped<CentersService>();
builder.Services.AddScoped<CenterInventoriesService>();
builder.Services.AddScoped<DonationsService>();
builder.Services.AddScoped<ReliefCategoriesService>();
builder.Services.AddScoped<ReliefItemsService>();
builder.Services.AddScoped<SosRequestsService>();
builder.Services.AddScoped<ThaiAddressesService>();
builder.Services.AddScoped<UploadService>();
builder.Services.AddScoped<WeatherForecastService>();
builder.Services.AddScoped<NotificationsService>();
builder.Services.AddScoped<NotificationRealtimeService>();

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "ไม่พบ ConnectionStrings:DefaultConnection"
    );

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    );
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",
                "http://127.0.0.1:3000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("ไม่พบ Jwt:Key");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("ไม่พบ Jwt:Issuer");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("ไม่พบ Jwt:Audience");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,

                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey)
                ),

                ClockSkew = TimeSpan.Zero
            };

        // SignalR browser clients send the JWT as access_token during
        // WebSocket/SSE transport. Only accept it for our notification hub.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (
                    !string.IsNullOrWhiteSpace(accessToken) &&
                    path.StartsWithSegments("/hubs/notifications")
                )
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "Flood Relief API",
            Version = "v1"
        }
    );

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "กรอก Token อย่างเดียว ไม่ต้องใส่คำว่า Bearer"
        }
    );

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        }
    );
});

var app = builder.Build();

/*
 * ส่ง Error กลับเป็น JSON เสมอ
 * ป้องกัน Frontend เจอ Unexpected token 'S'
 */
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode =
            StatusCodes.Status500InternalServerError;

        context.Response.ContentType =
            "application/json; charset=utf-8";

        var response = new
        {
            message = "เกิดข้อผิดพลาดภายในระบบ",
            detail = app.Environment.IsDevelopment()
                ? "กรุณาตรวจสอบ Console ของ Backend"
                : null
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response)
        );
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Flood Relief API v1"
        );

        options.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
