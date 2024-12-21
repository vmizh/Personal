namespace Personal.Data.Repositories;

public interface IBaseRepository<T>
{
    Task CreateAsync(T entity);
    Task CreateManyAsync(IEnumerable<T> entities);
    Task UpdateAsync(T entity);
    Task UpdateManyAsync(IEnumerable<T> entities);
    Task DeleteAsync(Guid entity);
    Task DeleteManyAsync(IEnumerable<Guid> ids);
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
}
