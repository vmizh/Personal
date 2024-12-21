using Personal.Data.Repositories;
using Personal.Domain.Entities;

namespace Personal.Services.Services;

public class AuthorService(IBaseRepository<Author> repository) : BaseService<Author>(repository), IAuthorService
{
    protected override string RepositoryName => "Репозиторий авторов";
}
