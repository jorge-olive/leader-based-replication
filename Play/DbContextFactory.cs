using Microsoft.EntityFrameworkCore;
using POC;
//SINGLETON
class DbContextFactory
{
    private IEnumerable<string> _replicasConnectionStrings;
    private string _masterConnectionString;
    private ConcurrentCircularBuffer<string> _replicasConnectionBuffer;
    public DbContextFactory(IConfiguration configuration)
    {
        _replicasConnectionStrings = 
            configuration.GetSection("ReadOnlyReplicas").Get<string[]>();

        _masterConnectionString = configuration.GetConnectionString("BloggingContextMaster");

        _replicasConnectionBuffer = new ConcurrentCircularBuffer<string>(_replicasConnectionStrings.Count(), _replicasConnectionStrings.ToArray());
    }

    public ReadOnlyDbContext GetReadOnlyDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ReadOnlyDbContext>();
        optionsBuilder.UseNpgsql(_replicasConnectionBuffer.GetNext());

        return new ReadOnlyDbContext(optionsBuilder.Options);
    }

    public WriteDbContext GetApplicationDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<WriteDbContext>();
        optionsBuilder.UseNpgsql(_masterConnectionString);

        return new WriteDbContext(optionsBuilder.Options);
    }

    //TODO LoadBalancer
    //Remove unhealthy replicas by replication lag greater than X
    //Remove unreachable replicas 
}

