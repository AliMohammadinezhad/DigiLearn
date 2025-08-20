using BlogModule.Context;
using BlogModule.Domain;
using Common.Infrastructure.Repository;

namespace BlogModule.Repository.Posts;

public class PostRepository : BaseRepository<Post, BlogContext>, IPostRepository
{
    private readonly BlogContext _context;
    public PostRepository(BlogContext context) : base(context)
    {
        _context = context;
    }

    public Task DeletePost(Post post)
    {
        _context.Posts.Remove(post);
        return Task.CompletedTask;
    }
}