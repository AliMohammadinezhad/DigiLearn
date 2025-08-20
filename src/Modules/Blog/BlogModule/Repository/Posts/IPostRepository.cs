using BlogModule.Domain;
using Common.Domain.Repository;

namespace BlogModule.Repository.Posts;

public interface IPostRepository : IBaseRepository<Post>
{
    Task DeletePost(Post post);
}