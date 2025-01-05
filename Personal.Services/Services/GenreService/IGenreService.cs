using Microsoft.AspNetCore.Http;
using Personal.Data.Repositories;
using Personal.Domain.Entities;

namespace Personal.Services.Services;

public interface IGenreService : IBaseService<Genre>
{
    
}

public class GenreService(IBaseRepository<Genre> repository, IGenreRepository genreRepository) : BaseService<Genre>(repository), IGenreService
{
    protected override string RepositoryName => "Репозиторий типов литературы";
    private IGenreRepository myGenreRepository = genreRepository;

    public override async Task<IResult> UpdateAsync(Genre item)
    {
        var res = await base.UpdateAsync(item);
        //await myGenreRepository.UpdateReferencesAsync(item._id);
        return res;
    }
}

