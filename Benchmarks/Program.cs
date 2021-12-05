using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.EntityFrameworkCore;
using Seeder;

var summary = BenchmarkRunner.Run<SplitQueryBenchmarking>();

[RPlotExporter]
public class SplitQueryBenchmarking
{
    private MyDbContext _context;

    [Params(1)]
    public int BlogId;

    [GlobalSetup]
    public void Setup()
    {
        var dbContextOptions = new DbContextOptionsBuilder<MyDbContext>()
        .UseNpgsql("Host=localhost;Database=my_db;Username=postgres;Password=testing");
        _context = new MyDbContext(dbContextOptions.Options);
    }

    [Benchmark]
    public async Task SplitQuery()
    {
        var blog = await _context.Blogs.AsSplitQuery().Include(x => x.Posts).ThenInclude(x => x.Comments).Where(x => x.BlogId == BlogId).ToListAsync();
    }

    [Benchmark]
    public async Task NonSplitQuery()
    {
        var blog = await _context.Blogs.Include(x => x.Posts).ThenInclude(x => x.Comments).Where(x => x.BlogId == BlogId).ToListAsync();
    }
}
