using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TicketModule.Core.Services;
using TicketModule.Data;

namespace TicketModule;

public static class TicketBootstrapper
{
    public static IServiceCollection InitBlogModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TicketContext>(builder =>
        {
            builder.UseSqlServer(configuration.GetConnectionString("Ticket_Context"));
        });
        services.AddScoped<ITicketService, TicketService>();
        services.AddAutoMapper(typeof(MapperProfile).Assembly);
        return services;
    }
}