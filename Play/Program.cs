using Microsoft.EntityFrameworkCore;
using Seeder;

var hostBuilder = Host.CreateDefaultBuilder(args);

hostBuilder.ConfigureServices((ctx, services) =>
{
    services.AddDbContext<MyDbContext>(options =>
            options.UseNpgsql(ctx.Configuration.GetConnectionString("BloggingContext")));

    services.AddScoped<RandomDataGenerator>();
})
    .ConfigureLogging((hostingContext, logging) =>
    {
        logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
        logging.AddConsole();
    });

var app = hostBuilder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
    db.Database.Migrate();

    var dataSeeder = scope.ServiceProvider.GetRequiredService<RandomDataGenerator>();
    await dataSeeder.Run(CancellationToken.None);
}
