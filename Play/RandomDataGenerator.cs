using Bogus;

namespace POC
{
    public class RandomDataGenerator
    {
        public RandomDataGenerator(IServiceScopeFactory prv)
        {
            this.prv = prv;
        }

        public IServiceScopeFactory prv { get; }

        public async Task Run(CancellationToken cancellationToken)
        {
            using (var dbContext = prv.CreateScope().ServiceProvider.GetService<DbContextFactory>()!.GetApplicationDbContext())
            {
                var commentFaker = new Faker<Comment>()
                .RuleFor(o => o.Content, f => f.Lorem.Sentence(1, 120));

                var postFaker = new Faker<Post>()
                    .RuleFor(o => o.Title, f => f.Lorem.Sentence(1, 12))
                    .RuleFor(o => o.Content, f => f.Lorem.Sentence(1, 120))
                    .RuleFor(o => o.Comments, f => commentFaker.Generate(f.Random.Number(1, 3)));

                var blogFaker = new Faker<Blog>()
                    .RuleFor(o => o.Name, f => f.Lorem.Sentence(12))
                    .RuleFor(o => o.Url, f => f.Internet.Url()).RuleFor(o => o.Posts, f => postFaker.Generate(f.Random.Number(1, 12)));

                    var blogs = blogFaker.Generate(Random.Shared.Next(1, 10));
                    
                    await dbContext!.Blogs.AddRangeAsync(blogs, cancellationToken);
                    await dbContext.SaveChangesAsync(cancellationToken);

                    Thread.Sleep(1000);               
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
