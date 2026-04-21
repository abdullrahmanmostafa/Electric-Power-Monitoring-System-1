using Electric_Power_Monitoring_System.Areas.Identity.Data;
using Electric_Power_Monitoring_System.Repositories;
using Electric_Power_Monitoring_System.Services;
using Microsoft.EntityFrameworkCore;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

var builder = WebApplication.CreateBuilder(args);

//
// 🔥 Firebase Setup (LOCAL + RAILWAY SAFE)
//
var firebaseJson = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS");

if (!string.IsNullOrEmpty(firebaseJson))
{
    // Railway / Production
    FirebaseApp.Create(new AppOptions()
    {
        Credential = GoogleCredential.FromJson(firebaseJson)
    });
}
else
{
    // Local fallback
    var firebaseJsonPath = Path.Combine(builder.Environment.ContentRootPath, "Firebase", "firebase-service-account.json");

    if (File.Exists(firebaseJsonPath))
    {
        FirebaseApp.Create(new AppOptions()
        {
            Credential = GoogleCredential.FromFile(firebaseJsonPath)
        });
    }
}

//
// Controllers + Swagger
//
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//
// Database (PostgreSQL)
//
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new Exception("Database connection string is missing!");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

//
// Repositories
//
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IHubRepository, HubRepository>();
builder.Services.AddScoped<IPlugRepository, PlugRepository>();
builder.Services.AddScoped<IReadingRepository, ReadingRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IUserDeviceRepository, UserDeviceRepository>();

//
// Services
//
builder.Services.AddSingleton<IFcmSender, FcmSenderV1>();
builder.Services.AddHostedService<AlertBackgroundService>();

var app = builder.Build();

//
// Swagger (dev only)
//
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

//
// Middleware
//
//app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

//
// Auto apply migrations (IMPORTANT for Railway)
//
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();