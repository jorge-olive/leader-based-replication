using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Seeder;

var hostBuilder = Host.CreateDefaultBuilder(args);

hostBuilder.ConfigureServices((ctx, services) =>
{
    services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(ctx.Configuration.GetConnectionString("BloggingContextMaster")));

    services.AddDbContext<ReadOnlyDbContext>(options =>
        options.UseNpgsql(ctx.Configuration.GetConnectionString("BloggingContextReadOnly")));

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
            var dbContext = scope.ServiceProvider.GetRequiredService<ReadOnlyDbContext>();
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

class DbContextFactory
{
    private IEnumerable<string> _replicasConnectionStrings;
    private string _masterConnectionString;
    private ConcurrentCircularBuffer<string> _replicasConnectionBuffer;
    public DbContextFactory(IConfiguration configuration)
    {
        _replicasConnectionStrings = 
            configuration.GetSection("ConnectionStrings").GetValue<IEnumerable<string>>("ReadOnlyReplicas");

        _masterConnectionString = configuration.GetConnectionString("master");

        _replicasConnectionBuffer = new ConcurrentCircularBuffer<string>(_replicasConnectionStrings.Count(), _replicasConnectionStrings.ToArray());
    }

    public ReadOnlyDbContext GetReadOnlyDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ReadOnlyDbContext>();
        optionsBuilder.UseNpgsql(_replicasConnectionBuffer.GetNext());

        return new ReadOnlyDbContext(optionsBuilder.Options);
    }

    public AppDbContext GetApplicationDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(_masterConnectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}

class ConcurrentCircularBuffer<T>
{
    private int _capacity;
    private int _currentPosition;
    private T[] _elements;
    private readonly object _lock = new object();

    public ConcurrentCircularBuffer(int capacity,T[] elements)
    {
        this._capacity = capacity;
        this._elements = elements;
    }

    public T GetNext()
    {
        lock (_lock)
        {
            if (_currentPosition < _capacity - 1)
            {
                _currentPosition++;
                return _elements[_currentPosition];
            }

            _currentPosition = 0;
            return _elements[0];
        }
    }
}

