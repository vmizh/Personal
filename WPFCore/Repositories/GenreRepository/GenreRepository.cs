using Personal.Domain.Entities;
using Personal.WPFClient.Repositories.Base;
using WPFClient.Configuration;

namespace WPFCore.Repositories;

public class GenreRepository : BaseRepository<Genre>, IGenreRepository
{
    public GenreRepository(ServiceConfigurationBuilder config) : base(config)
    {
        Endpoint = "Genre";
    }
}
