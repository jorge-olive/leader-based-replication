using Microsoft.EntityFrameworkCore;
using Seeder;

var hostBuilder = Host.CreateDefaultBuilder(args);

hostBuilder.ConfigureServices((ctx, services) =>
{
    services.AddSingleton<DbContextFactory>();

    //services.AddDbContext<ReadOnlyDbContext>(options =>
    //    options.UseNpgsql(ctx.Configuration.GetConnectionString("BloggingContextReadOnly")));

    services.AddScoped<RandomDataGenerator>();
    
})
    .ConfigureLogging((hostingContext, logging) =>
    {
        logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
        logging.AddConsole();
    });


var app = hostBuilder.Build();

//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//    db.Database.Migrate();
//}

var continuousQueryAsync = Task.Run(async () =>
{
    var random = Random.Shared;
    var range = Enumerable.Range(random.Next(1, 10000), random.Next(1, 20));

    while (true)
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<DbContextFactory>().GetReadOnlyDbContext();
            var blogs = await dbContext.Posts.Where(x => x.Content.StartsWith("A")).ToListAsync();
        }
});

var creationNewBlogs = Task.Run(async () =>
{
    while (true)
        using (var scope = app.Services.CreateScope())
        {
            var dataSeeder = scope.ServiceProvider.GetRequiredService<RandomDataGenerator>();
            await dataSeeder.Run(CancellationToken.None);
        }
});

await Task.WhenAll(continuousQueryAsync);