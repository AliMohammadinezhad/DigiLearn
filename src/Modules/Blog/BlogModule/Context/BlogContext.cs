using BlogModule.Domain;
using Microsoft.EntityFrameworkCore;

namespace BlogModule.Context;

public class BlogContext : DbContext
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<Post> Posts { get; set; }


    public BlogContext(DbContextOptions<BlogContext> options) : base(options)
    {
        
    }
}