using AutoMapper;
using BlogModule.Context;
using BlogModule.Repository.Categories;
using BlogModule.Repository.Posts;
using BlogModule.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlogModule;

public static class BlogBootstrapper
{
    public static IServiceCollection InitBlogModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BlogContext>(builder =>
        {
            builder.UseSqlServer(configuration.GetConnectionString("Blog_Context"));
        });
        services.AddScoped<IBlogService, BlogService>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddAutoMapper(typeof(MapperProfile).Assembly);
        return services;
    }
}