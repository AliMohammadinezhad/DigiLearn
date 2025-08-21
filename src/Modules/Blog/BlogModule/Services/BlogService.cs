using AutoMapper;
using BlogModule.Domain;
using BlogModule.Repository.Categories;
using BlogModule.Repository.Posts;
using BlogModule.Services.DTOs.Command;
using BlogModule.Services.DTOs.Query;
using BlogModule.Utils;
using Common.Application;
using Common.Application.FileUtil.Interfaces;
using Common.Application.SecurityUtil;

namespace BlogModule.Services;

public class BlogService : IBlogService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IPostRepository _postRepository;
    private readonly IMapper _mapper;
    private readonly IFileService _localFileService;

    public BlogService(ICategoryRepository categoryRepository, IMapper mapper, IPostRepository postRepository, IFileService localFileService)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _postRepository = postRepository;
        _localFileService = localFileService;
    }

    #region Category
    public async Task<OperationResult> CreateCategory(CreateCategoryCommand command)
    {
        Category? category = _mapper.Map<Category>(command);
        if (category is null)
            return OperationResult.NotFound();

        if (await _categoryRepository.ExistsAsync(x => x.Slug == command.Slug))
            return OperationResult.Error("Slug is Exist");

        await _categoryRepository.AddAsync(category);
        await _categoryRepository.Save();

        return OperationResult.Success();
    }

    public async Task<OperationResult> EditCategory(EditCategoryCommand command)
    {
        var category = await _categoryRepository.GetAsync(command.Id);
        if (category is null)
            return OperationResult.NotFound();

        if (category.Slug != command.Slug)
        {
            if (await _categoryRepository.ExistsAsync(x => x.Slug == command.Slug))
                return OperationResult.Error("Slug is Exist");
        }

        category.Slug = command.Slug;
        category.Title = command.Title;

        _categoryRepository.Update(category);
        await _categoryRepository.Save();

        return OperationResult.Success();
    }

    public async Task<OperationResult> DeleteCategory(Guid categoryId)
    {
        var category = await _categoryRepository.GetAsync(categoryId);
        if (category is null)
            return OperationResult.NotFound();

        if (await _postRepository.ExistsAsync(x => x.CategoryId == categoryId))
            return OperationResult.Error("There Is Some Posts In Chis Category");

        await _categoryRepository.RemoveCategory(category);
        await _categoryRepository.Save();
        return OperationResult.Success();
    }

    public async Task<List<BlogCategoryDto>> GetAllCategoriesList()
    {
        var categories = await _categoryRepository.GetAllCategories();
        return _mapper.Map<List<BlogCategoryDto>>(categories);
    }

    public async Task<BlogCategoryDto> GetCategoryById(Guid categoryId)
    {
        var category = await _categoryRepository.GetAsync(categoryId);
        return _mapper.Map<BlogCategoryDto>(category);
    }
    #endregion


    #region Post
    public async Task<OperationResult> CreatePost(CreatePostCommand command)
    {
        var post = _mapper.Map<Post>(command);
        if(post is null)
            return OperationResult.NotFound();

        if (await _postRepository.ExistsAsync(x => x.Slug == command.Slug))
            return OperationResult.Error("Slug Is Exist");

        if(command.ImageFile.IsImage())
            return OperationResult.Error("Image Have Problem");

        var imageName = await _localFileService.SaveFileAndGenerateName(command.ImageFile, BlogDirectories.PostImage);
        post.ImageName = imageName;
        post.Visit = 1;
        post.Description = post.Description.SanitizeText();

        await _postRepository.AddAsync(post);
        await _postRepository.Save();
        return OperationResult.Success();
    }

    public async Task<OperationResult> EditPost(EditPostCommand command)
    {
        var post = await _postRepository.GetTracking(command.Id);
        
        if(post is null)
            return OperationResult.NotFound();
        
        if(post.Slug != command.Slug)
            if (await _postRepository.ExistsAsync(x => x.Slug == command.Slug))
                return OperationResult.Error("Slug Is Exist");

        if(command.ImageFile is not null)
            if(command.ImageFile.IsImage() == false)
                return OperationResult.Error("The Image File Is Invalid");
            else
            {
                var imageName = await _localFileService.SaveFileAndGenerateName(command.ImageFile, BlogDirectories.PostImage);
                post.ImageName = imageName;
            }

        post.OwnerName = command.OwnerName;
        post.Description = command.Description.SanitizeText();
        post.Title = command.Title;
        post.CategoryId = command.CategoryId;
        post.UserId = command.UserId;

        await _postRepository.Save();
        return OperationResult.Success();

    }

    public async Task<OperationResult> DeletePost(Guid postId)
    {
        var post = await _postRepository.GetTracking(postId);
        if(post is null)
            return OperationResult.NotFound();

        await _postRepository.DeletePost(post);
        _localFileService.DeleteFile(BlogDirectories.PostImage, post.ImageName);
        await _postRepository.Save();
        return OperationResult.Success();
    }

    public async Task<BlogPostDto> GetPostById(Guid postId)
    {
        var post = await _postRepository.GetTracking(postId);
        if (post is null)
            return new();
        var postDto = _mapper.Map<BlogPostDto>(post);
        return postDto;
    }
    #endregion

}