using MeetingService.Application.Interfaces;
using MeetingService.Domain.Interfaces;
using MeetingService.Infrastructure;
using MeetingService.Infrastructure.Messaging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MeetingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IMeetingService, MeetingService.Application.Implement.MeetingService>();
builder.Services.AddHostedService<OutboxService>();
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "TLUMeet",
            ValidAudience = "TLUMeet",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MeetingDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    var retries = 5;
    while (retries > 0)
    {
        try
        {
            db.Database.Migrate();
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
app.UseCors("AllowAll");
app.UseAuthentication();

app.UseAuthorization();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();


}


app.MapControllers();

app.Run();