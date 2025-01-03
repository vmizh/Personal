namespace Personal.Data.Repositories;

public interface IAuthorRepository
{
    /// <summary>
    /// Обновляет значения в старых ссылках
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task UpdateReferencesAsync(Guid id);
}
