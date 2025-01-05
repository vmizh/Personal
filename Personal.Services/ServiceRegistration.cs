using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Personal.Services.Services;

namespace Personal.Services;

public static class ServiceRegistration
{
    public static IServiceCollection AddServiceDependencies(this IServiceCollection services,
        IConfiguration configuration)
    {
        //services.AddScoped<IBaseService<Country>, BaseService<Country>>();
        services.AddScoped<ICountriesService, CountriesService>();
        services.AddScoped<IAuthorService, AuthorService>();
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IReadPagingService, ReadPagingService>();
        services.AddScoped<ILayoutService, LayoutService>();
        services.AddScoped<IBookPartitionsService, BookPartitionsService>();
        services.AddScoped<IGenreService, GenreService>();

        return services;
    }
}
