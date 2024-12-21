using Personal.WPFClient.Repositories.Base;
using WPFClient.Configuration;

namespace Personal.WPFClient.Repositories;

public class AuthorRepository : BaseRepository<Domain.Entities.Author>, IAuthorRepository
{
    public AuthorRepository(ServiceConfigurationBuilder config) : base(config)
    {
        Endpoint = "Author";
    }
}
