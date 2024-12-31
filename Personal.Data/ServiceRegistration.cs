using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Personal.Data.Repositories;
using Personal.Domain.Config;
using Personal.Domain.Entities;

namespace Personal.Data;

public static class ServiceRegistration
{
    public static IServiceCollection AddDataDependencies(this IServiceCollection services,
        IConfiguration configuration)
    { 
        services.AddDbContext<MongoDBContext>(options =>
            options.UseMongoDB(configuration.GetSection("MongoDbSettings:ConnectionString").Value ?? "",
                configuration.GetSection("MongoDbSettings:DatabaseName").Value ?? ""));

        services.AddDbContext<LayoutContext>(options =>
            options.UseMongoDB(configuration.GetSection("MongoDbLayoutSettings:ConnectionString").Value ?? "",
                configuration.GetSection("MongoDbLayoutSettings:DatabaseName").Value ?? ""));

        services.AddScoped<IBaseRepository<Country>, BaseRepository<Country>>();
        services.AddScoped<ICountriesRepository, CountriesRepository>();

        services.AddScoped<IBaseRepository<Author>, BaseRepository<Author>>();
        services.AddScoped<IAuthorRepository, AuthorRepository>();

        services.AddScoped<IBaseRepository<Book>, BaseRepository<Book>>();
        services.AddScoped<IBookRepository, BookRepository>();

        services.AddScoped<IBaseRepository<ReadPaging>, BaseRepository<ReadPaging>>();
        services.AddScoped<IReadPagingRepository, ReadPagingRepository>();

        services.AddScoped<IBaseLayoutRepository<Layout>, BaseLayoutRepository<Layout>>();
        services.AddScoped<ILayoutRepository, LayoutRepository>();

        services.AddScoped<IBaseRepository<BookPartition>, BaseRepository<BookPartition>>();
        services.AddScoped<IBookPartitionsRepository, BookPartitionsRepository>();

        //services.AddScoped<IUserRepository, UserRepository>();
        return services;
    }
}
