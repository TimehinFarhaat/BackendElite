using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Npgsql;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory(),
    EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
});

var env = builder.Environment;

// 🔹 Load Configuration
builder.Configuration
    .SetBasePath(env.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables();

// Database
var connectionString = builder.Configuration.GetConnectionString(
    env.IsDevelopment() ? "DefaultConnection" : "PostgresConnection");

Console.WriteLine($"📦 Using connection string: {connectionString}");
Console.WriteLine($"🌍 Environment: {env.EnvironmentName}");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (env.IsDevelopment())
        options.UseSqlServer(connectionString);
    else
        options.UseNpgsql(connectionString);
});

// 🔹 Cookie Authentication & Session
builder.Services.AddHttpClient();
builder.Services.AddScoped<ClarifaiService>();
builder.Services.AddHttpClient<ClarifaiService>();

builder.Services.Configure<ClarifaiSettings>(builder.Configuration.GetSection("ClarifaiSettings"));
builder.Services.Configure<AdminSettings>(builder.Configuration.GetSection("AdminSettings"));

// Services & DI
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<CarRepository>();
builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<IInquiryService, InquiryService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.IdleTimeout = TimeSpan.FromHours(1);
});

// 🔹 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.WithOrigins("https://localhost:7238") // frontend dev port
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });

    options.AddPolicy("ProdCors", policy =>
    {
        policy.WithOrigins("https://elitecars-api.onrender.com") // your Render frontend
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// 🔹 Middleware Pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Use correct CORS based on environment
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevCors");
}
else
{
    app.UseCors("ProdCors");
}

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure DB created & migrations applied
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    //db.Database.Migrate();
}

app.Run();
