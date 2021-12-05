using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Seeder
{
    public class BlogEntityTypeConfiguration : IEntityTypeConfiguration<Blog>
    {
        public void Configure(EntityTypeBuilder<Blog> builder)
        {
            builder
                .Property(b => b.Url)
                .IsRequired();

            builder.HasKey(b => b.BlogId);

            builder
                .HasMany(b => b.Posts)
                .WithOne(b => b.Blog).
                HasForeignKey(b => b.BlogId);
        }
    }

    public class PostEntityTypeConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.HasKey(b => b.PostId);

            builder
                .HasMany(b => b.Comments)
                .WithOne(b => b.Post).
                HasForeignKey(b => b.PostId);
        }
    }

    public class CommentEntityTypeConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.HasKey(b => b.CommentId);
        }
    }
}
