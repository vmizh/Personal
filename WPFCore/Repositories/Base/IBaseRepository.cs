using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Personal.WPFClient.Repositories.Base;

public interface IBaseRepository<T>
{
    Task<T> CreateAsync(T item);
    Task<IEnumerable<T>> CreateManyAsync(IEnumerable<T> items);
    Task<T> UpdateAsync(T item);
    Task<IEnumerable<T>> UpdateManyAsync(IEnumerable<T> items);
    Task<bool> DeleteAsync(T item);
    Task<bool> DeleteManyAsync(IEnumerable<T> items);
    Task<T> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
}
