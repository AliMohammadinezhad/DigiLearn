using BlogModule.Context;
using BlogModule.Domain;
using Common.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace BlogModule.Repository.Categories;

public class CategoryRepository : BaseRepository<Category, BlogContext> ,ICategoryRepository
{
    private readonly BlogContext _context;
    public CategoryRepository(BlogContext context) : base(context)
    {
        _context = context;
    }

    public Task RemoveCategory(Category category)
    {
        _context.Categories.Remove(category);
        return Task.CompletedTask;
    }

    public async Task<List<Category>> GetAllCategories()
    {
        return await _context.Categories.ToListAsync();
    }
}