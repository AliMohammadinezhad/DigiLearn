using BlogModule.Services.DTOs.Command;
using BlogModule.Services.DTOs.Query;
using Common.Application;

namespace BlogModule.Services;

public interface IBlogService
{
    #region Category
    Task<OperationResult> CreateCategory(CreateCategoryCommand command);
    Task<OperationResult> EditCategory(EditCategoryCommand command);
    Task<OperationResult> DeleteCategory(Guid categoryId);


    Task<List<BlogCategoryDto>> GetAllCategoriesList();
    Task<BlogCategoryDto> GetCategoryById(Guid categoryId);

    #endregion
    #region Post

    Task<OperationResult> CreatePost(CreatePostCommand command);
    Task<OperationResult> EditPost(EditPostCommand command);
    Task<OperationResult> DeletePost(Guid postId);

    Task<BlogPostDto> GetPostById(Guid postId);

    #endregion
}