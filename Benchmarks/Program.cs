using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.EntityFrameworkCore;
using Seeder;

var summary = BenchmarkRunner.Run<SplitQueryBenchmarking>();

[RPlotExporter]
[MemoryDiagnoser]
public class SplitQueryBenchmarking
{
    private MyDbContext _context;

    [Params(1)]
    public int BlogId;

    [GlobalSetup]
    public void Setup()
    {
        var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
        .UseNpgsql("Host=localhost:5432;Database=postgres;Username=postgres;Password=secret");
        _context = new AppDbContext(dbContextOptions.Options);
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
