using Microsoft.EntityFrameworkCore;
using POC;

var hostBuilder = Host.CreateDefaultBuilder(args);

hostBuilder.ConfigureServices((ctx, services) =>
{
    services.AddSingleton<DbContextFactory>();
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
    var db = scope.ServiceProvider.GetRequiredService<DbContextFactory>().GetApplicationDbContext();
    db.Database.Migrate();
}

var continuousDataCreation = Parallel.ForEachAsync(Enumerable.Range(1, 2), new ParallelOptions() { MaxDegreeOfParallelism = 2 }, async (value, token) =>
{
    while (true)
        using (var scope = app.Services.CreateScope())
        {
            var dataSeeder = scope.ServiceProvider.GetRequiredService<RandomDataGenerator>();
            await dataSeeder.Run(CancellationToken.None);
            Thread.Sleep(Random.Shared.Next(100, 2000));
        }
});

var continuousQueryLoad = Parallel.ForEachAsync(Enumerable.Range(1,100), new ParallelOptions() {  MaxDegreeOfParallelism = 100 }, async (value, token)  =>
{
    var random = Random.Shared;
    var range = Enumerable.Range(random.Next(1, 10000), random.Next(1, 20));

    while (true)
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<DbContextFactory>().GetReadOnlyDbContext();
            await dbContext.Posts.Skip(Random.Shared.Next(1, 1000)).Take(Random.Shared.Next(1,20)).Include(x => x.Comments).ToListAsync();
        }
});

//INFINITE RUN LOOP
await Task.WhenAll(continuousQueryLoad, continuousDataCreation);