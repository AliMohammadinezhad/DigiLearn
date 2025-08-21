using System.Linq.Expressions;
using AutoMapper;
using BlogModule.Domain;
using BlogModule.Repository.Categories;
using BlogModule.Repository.Posts;
using BlogModule.Services;
using BlogModule.Services.DTOs.Command;
using BlogModule.Services.DTOs.Query;
using Common.Application;
using Common.Application.FileUtil.Interfaces;
using Moq;

namespace BlogModule.Unit.Test;

public class CategoryTest
{
    private readonly Mock<ICategoryRepository> _categoryRepoMock;
    private readonly Mock<IPostRepository> _postRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly BlogService _service;
    private static readonly Guid CategoryId = Guid.NewGuid();

    public CategoryTest()
    {
        _categoryRepoMock = new Mock<ICategoryRepository>();
        _postRepoMock = new Mock<IPostRepository>();
        _mapperMock = new Mock<IMapper>();
        _service = new BlogService(
            _categoryRepoMock.Object,
            _mapperMock.Object,
            _postRepoMock.Object, // quick dummy instead of full mock
            Mock.Of<IFileService>() // quick dummy
        );
    }

    #region Create

    [Fact]
    public async Task CreateCategory_ShouldReturnSuccess_When_CategoryIsNew()
    {
        // Arrange
        var command = new CreateCategoryCommand { Slug = "tech", Title = "Technology" };
        var category = new Category { Slug = command.Slug };

        TestMapping(command, category);
        TestSlugExists(false);

        // Act
        var result = await _service.CreateCategory(command);

        // Assert
        Assert.True(result.Status == OperationResultStatus.Success);
        _categoryRepoMock.Verify(r => r.AddAsync(It.Is<Category>(c => c.Slug == "tech")), Times.Once);
        _categoryRepoMock.Verify(r => r.Save(), Times.Once);
    }

    [Fact]
    public async Task CreateCategory_ShouldReturnNotFound_When_CategoryIsNull()
    {
        // Arrange
        var command = new CreateCategoryCommand { Slug = "tech", Title = "Technology" };

        TestMapping(command, (Category)null!);
        TestSlugExists(false);

        // Act
        var result = await _service.CreateCategory(command);

        // Assert
        Assert.True(result.Status == OperationResultStatus.NotFound);
    }

    [Theory]
    [InlineData("tech", false, true)]
    [InlineData("news", true, false)]
    public async Task CreateCategory_ValidatesSlug(string slug, bool slugExistence, bool finalOutput)
    {
        // Arrange
        var command = new CreateCategoryCommand { Slug = slug, Title = slug.Replace("-", " ") };
        var category = new Category { Slug = slug, Title = slug.Replace("-", " ") };

        TestMapping(command, category);
        TestSlugExists(slugExistence);

        // Act
        var result = await _service.CreateCategory(command);

        // Assert
        Assert.Equal(finalOutput, result.Status == OperationResultStatus.Success);
    }
    #endregion
    #region Edit

    [Fact]
    public async Task EditCategory_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        var command = new EditCategoryCommand { Id = Guid.NewGuid(), Slug = "tech" };
        _categoryRepoMock.Setup(r => r.GetAsync(command.Id)).ReturnsAsync((Category)null);

        // Act
        var result = await _service.EditCategory(command);

        // Assert
        Assert.Equal(OperationResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task EditCategory_ShouldReturnError_WhenCategorySlugAlreadyExist()
    {
        // Arrange

        var command = new EditCategoryCommand { Id = CategoryId, Slug = "new" };
        var existingCategory = new Category { Id = CategoryId, Slug = "old" };
        _categoryRepoMock.Setup(r => r.GetAsync(command.Id)).ReturnsAsync(existingCategory);
        _categoryRepoMock.Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<Category, bool>>>()))
            .ReturnsAsync(true);
        // Act
        var result = await _service.EditCategory(command);

        // Assert
        Assert.Equal(OperationResultStatus.Error, result.Status);
        _categoryRepoMock.Verify(r => r.Update(It.IsAny<Category>()), Times.Never);
    }
    [Fact]
    public async Task EditCategory_ShouldReturnSuccess_WhenCategoryCanUpdateCorrectly()
    {
        // Arrange
        var command = new EditCategoryCommand { Id = CategoryId, Slug = "new", Title = "New Title" };
        var existingCategory = new Category { Id = CategoryId, Slug = "old", Title = "Old Title" };
        _categoryRepoMock.Setup(r => r.GetAsync(command.Id)).ReturnsAsync(existingCategory);
        _categoryRepoMock.Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<Category, bool>>>()))
            .ReturnsAsync(false);
        // Act
        var result = await _service.EditCategory(command);

        // Assert
        Assert.Equal(OperationResultStatus.Success, result.Status);
        Assert.Equal("new", command.Slug);
        Assert.Equal("New Title", command.Title);
        _categoryRepoMock.Verify(r => r.Update(existingCategory), Times.Once);
        _categoryRepoMock.Verify(r => r.Save(), Times.Once);
    }

    #endregion
    #region Delete

    [Fact]
    public async Task DeleteCategory_ShouldReturnNotFound_WhenCategoryIsNull()
    {
        _categoryRepoMock.Setup(x => x.GetAsync(CategoryId))
            .ReturnsAsync(null as Category);

        var result = await _service.DeleteCategory(CategoryId);

        Assert.Equal(OperationResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task DeleteCategory_ShouldReturnError_WhenCategoryContainsPost()
    {
        // Arrange
        var existedCategory = new Category { Id = CategoryId, Slug = "old" };
        _categoryRepoMock.Setup(r => r.GetAsync(CategoryId))
            .ReturnsAsync(existedCategory);
        _postRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Post, bool>>>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteCategory(CategoryId);

        Assert.Equal(OperationResultStatus.Error, result.Status);
        _categoryRepoMock.Verify(r => r.RemoveCategory(It.IsAny<Category>()), Times.Never);
        _categoryRepoMock.Verify(r => r.Save(), Times.Never);
    }
    [Fact]
    public async Task DeleteCategory_ShouldReturnSuccess_WhenCategoryValidToDelete()
    {
        // Arrange
        var existedCategory = new Category { Id = CategoryId, Slug = "old" };
        _categoryRepoMock.Setup(r => r.GetAsync(CategoryId))
            .ReturnsAsync(existedCategory);
        _postRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Post, bool>>>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.DeleteCategory(CategoryId);

        // Assert
        Assert.Equal(OperationResultStatus.Success, result.Status);
        _categoryRepoMock.Verify(r => r.RemoveCategory(It.IsAny<Category>()), Times.Once);
        _categoryRepoMock.Verify(r => r.Save(), Times.Once);
    }
    #endregion

    #region GetAllCategories

    [Fact]
    public async Task GetAllCategoriesList_ShouldReturnCategoryDtoList_WhenItIsValid()
    {
        // Arrange
        List<BlogCategoryDto> categoryDtoList =
        [
            new() { Slug = "tech" },
            new() { Slug = "news" }, 
            new() { Slug = "life" }
        ];
        List<Category> categoryList =
        [
            new() { Slug = "tech" },
            new() { Slug = "news" },
            new() { Slug = "life" }
        ];


        _categoryRepoMock.Setup(x => x.GetAllCategories())
            .ReturnsAsync(categoryList);

        TestMapping(categoryList, categoryDtoList);

        // Act
        var result = await _service.GetAllCategoriesList();


        // Assert
        Assert.Collection(result,
            dto => Assert.Equal("tech", dto.Slug),
            dto => Assert.Equal("news", dto.Slug),
            dto => Assert.Equal("life", dto.Slug)
        );
    }

    #endregion

    #region GetSingleCategory

    [Fact]
    public async Task GetCategoryById_ShouldReturnSingleCategory_WhenItIsValid()
    {
        // Arrange
        var category = new Category {Id = CategoryId, Slug = "foo" };
        var categoryDto = new BlogCategoryDto {Id = CategoryId, Slug = "foo" };

        _categoryRepoMock.Setup(x => x.GetAsync(CategoryId))
            .ReturnsAsync(category);

        TestMapping(category, categoryDto);

        // Act
        var result = await _service.GetCategoryById(CategoryId);

        // Assert
        Assert.Equal(categoryDto, result);
    }

    #endregion
    private void TestMapping<TSource, TDestination>(TSource input, TDestination output)
    {
        _mapperMock
            .Setup(m => m.Map<TDestination>(It.IsAny<TSource>()))
            .Returns(output);
    }


    private void TestSlugExists(bool exists)
    {
        _categoryRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Category, bool>>>()))
            .ReturnsAsync(exists);
    }


}