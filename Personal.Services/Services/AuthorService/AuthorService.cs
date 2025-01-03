using Microsoft.AspNetCore.Http;
using Personal.Data.Repositories;
using Personal.Domain.Entities;

namespace Personal.Services.Services;

public class AuthorService(IBaseRepository<Author> repository, IAuthorRepository authRepository) : BaseService<Author>(repository), IAuthorService
{
    protected override string RepositoryName => "Репозиторий авторов";
    private IAuthorRepository authRepository = authRepository;

    public override async Task<IResult> UpdateAsync(Author item)
    {
        var res = await base.UpdateAsync(item);
        await authRepository.UpdateReferencesAsync(item._id);
        return res;
    }
}
