using Electric_Power_Monitoring_System.Areas.Identity.Data;
using Electric_Power_Monitoring_System.Repositories;
using Electric_Power_Monitoring_System.Services;
using Microsoft.EntityFrameworkCore;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

var builder = WebApplication.CreateBuilder(args);
var firebaseJsonPath = Path.Combine(builder.Environment.ContentRootPath, "Firebase", "firebase-service-account.json");
if (File.Exists(firebaseJsonPath))
{
    FirebaseApp.Create(new AppOptions()
    {
        Credential = GoogleCredential.FromFile(firebaseJsonPath)
    });
}
else
{
    throw new FileNotFoundException($"Firebase service account JSON not found at {firebaseJsonPath}");
}
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IHubRepository, HubRepository>();
builder.Services.AddScoped<IPlugRepository, PlugRepository>();
builder.Services.AddScoped<IReadingRepository, ReadingRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IUserDeviceRepository, UserDeviceRepository>();

// Services
builder.Services.AddSingleton<IFcmSender, FcmSenderV1>(); 
builder.Services.AddHostedService<AlertBackgroundService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();