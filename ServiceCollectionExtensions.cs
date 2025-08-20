using Microsoft.EntityFrameworkCore;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackendServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {
        var connectionString = configuration.GetConnectionString(
            env.IsDevelopment() ? "DefaultConnection" : "PostgresConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (env.IsDevelopment())
                options.UseSqlServer(connectionString);
            else
                options.UseNpgsql(connectionString);
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<CarRepository>();
        services.AddScoped<ICarService, CarService>();
        services.AddScoped<IInquiryService, InquiryService>();
        services.AddScoped<IAdminService, AdminService>();

        services.Configure<ClarifaiSettings>(configuration.GetSection("ClarifaiSettings"));
        services.Configure<AdminSettings>(configuration.GetSection("AdminSettings"));

        return services;
    }
}
