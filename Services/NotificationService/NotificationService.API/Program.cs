using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NotificationService.API;
using NotificationService.API.Consumers;
using NotificationService.API.Hubs;
using NotificationService.API.Repository;
using NotificationService.API.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHostedService<WelcomeEmailConsumer>();
builder.Services.AddHostedService<PasswordResetConsumer>();
builder.Services.AddSignalR();
builder.Services.AddSingleton<INotificationService, NotiService>();
builder.Services.AddHostedService<MeetingInvitedConsumer>();
builder.Services.AddScoped<INotificationAppService, NotificationAppService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddHostedService<MeetingStartedConsumer>();
builder.Services.AddDbContext<NotificationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("NotificationDatabase"));
});
builder.Services.AddHostedService<InviteRespondedConsumer>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/hubs/notification"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
}
);
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", policy =>
        policy.WithOrigins("http://localhost:5173")  
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials())); 
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<NotificationHub>("/hubs/notification");
app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    var retries = 5;
    while (retries > 0)
    {
        try
        {
            dbContext.Database.Migrate();
            logger.LogInformation("Migration applied successfully");
            break;
        }
        catch (Exception ex)
        {
            retries--;
            logger.LogError(ex, "Migration failed, retrying...");
            Thread.Sleep(5000);
        }
    }
}
app.Run();
