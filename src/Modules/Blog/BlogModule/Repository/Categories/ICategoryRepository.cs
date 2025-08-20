using BlogModule.Domain;
using Common.Domain.Repository;

namespace BlogModule.Repository.Categories;

public interface ICategoryRepository : IBaseRepository<Category>
{
    Task RemoveCategory(Category category);
    Task<List<Category>> GetAllCategories();
}