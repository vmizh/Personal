﻿using Microsoft.AspNetCore.Http;
using Personal.Domain.Entities.Base;

namespace Personal.Services.Services;

public interface IBaseLayoutService<T> where T : IIdentity
{
    Task<IResult> CreateAsync(T item);
    Task<IResult> CreateManyAsync(IEnumerable<T> items);
    Task<IResult> UpdateAsync(T item);
    Task<IResult> UpdateManyAsync(IEnumerable<T> items);
    Task<IResult> DeleteAsync(Guid id);
    Task<IResult> DeleteManyAsync(IEnumerable<Guid> ids);
    Task<IResult> GetByIdAsync(Guid id);
    Task<IResult> GetAllAsync();
}
