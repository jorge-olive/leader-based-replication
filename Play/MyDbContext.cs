using Microsoft.EntityFrameworkCore;

namespace Seeder;

public class ReadOnlyDbContext : MyDbContext
{
    public ReadOnlyDbContext(DbContextOptions<ReadOnlyDbContext> options) : base(options)
    {
    }

    internal ReadOnlyDbContext(DbContextOptions options) : base(options)
    {
    }

    public override int SaveChanges()
    {
        throw new InvalidOperationException();
    }
}

public class AppDbContext : MyDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    internal AppDbContext(DbContextOptions options) : base(options)
    {
    }
}

public abstract class MyDbContext: DbContext
{
    protected MyDbContext(DbContextOptions<MyDbContext> options)
            : base(options)
    {
    }

    protected MyDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BlogEntityTypeConfiguration).Assembly);
    }
}

public class Blog
{
    public int BlogId { get; set; }
    public string Url { get; set; }
    public string Name { get; set; }
    public List<Post> Posts { get; set; }
}

public class Post
{
    public int PostId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public int BlogId { get; set; }
    public Blog Blog { get; set; }
    public List<Comment> Comments { get; set; }

}

public class Comment
{
    public int CommentId { get; set; }
    public string Content { get; set; }
    public Post Post { get; set; }
    public int PostId { get; set; }

}
