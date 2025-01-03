using System.Net;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Personal.Data.Repositories;
using Personal.Domain.Entities.Base;
using Personal.Services.Response;
using Serilog;

namespace Personal.Services.Services;

public class BaseService<T>(IBaseRepository<T> repository) : IBaseService<T>
    where T : IIdentity

{
    protected virtual string RepositoryName { set; get; } = "Базовый репозиторий";

    public virtual async Task<IResult> CreateAsync(T item)
    {
        var name = string.Empty;
        if(item is IName n)
            name = n.Name;
        Log.Logger.Information($"{RepositoryName}. Создание сущности {name}({item._id})");
        var response = new APIResponse();
        try
        {
            if (!Guid.Empty.Equals(item._id))
            {
                await repository.CreateAsync(item);
                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.OK;
                response.Result = item;
                return Results.Ok(response);
            }

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.NoContent;
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            return APIResponse.ReturnError(response, ex, Log.Logger);
        }
    }

    public virtual async Task<IResult> CreateManyAsync(IEnumerable<T> items)
    {
        Log.Logger.Information($"{RepositoryName}. Создание сущностей из списка");
        var response = new APIResponse();
        try
        {
            if (items.Any())
            {
                await repository.CreateManyAsync(items.ToList());
                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.OK;
                response.Result = true;
                return Results.Ok(response);
            }

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.NoContent;
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            return APIResponse.ReturnError(response, ex, Log.Logger);
        }
    }

    public virtual async Task<IResult> UpdateAsync(T item)
    {
        var name = string.Empty;
        if(item is IName n)
            name = n.Name;
        Log.Logger.Information($"{RepositoryName}. Обновление сущности {name}({item._id})");
        var response = new APIResponse();
        try
        {
            if (!Guid.Empty.Equals(item._id))
            {
                await repository.UpdateAsync(item);
                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.OK;
                response.Result = true;
                return Results.Ok(response);
            }

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.NoContent;
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            return APIResponse.ReturnError(response, ex, Log.Logger);
        }
    }
    
    public virtual async Task<IResult> UpdateManyAsync(IEnumerable<T> items)
    {
        
        Log.Logger.Information($"{RepositoryName}. Обновление сущностей из списка");
        var response = new APIResponse();
        try
        {
            if (items.Any())
            {
                await repository.UpdateManyAsync(items.ToList());
                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.OK;
                response.Result = true;
                return Results.Ok(response);
            }

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.NoContent;
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            return APIResponse.ReturnError(response, ex, Log.Logger);
        }
    }

    public virtual async Task<IResult> DeleteAsync(Guid id)
    {
        Log.Logger.Information($"{RepositoryName}. Удаление сущности с id='{id})'");
        var response = new APIResponse()
        {
            IsSuccess = false
        };
        try
        {
            if (!Guid.Empty.Equals(id))
            {
                await repository.DeleteAsync(id);
                
                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.OK;
                response.Result = true;
                return Results.Ok(response);
            }

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.NoContent;
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            return APIResponse.ReturnError(response, ex, Log.Logger);
        }
    }

    public virtual async Task<IResult> DeleteManyAsync(IEnumerable<Guid> ids)
    {
        Log.Logger.Information($"{RepositoryName}. Удаление сущностей из списка");
        var response = new APIResponse();
        try
        {
            if (ids.Any())
            {
                await repository.DeleteManyAsync(ids.ToList());
                
                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.OK;
                response.Result = true;
                return Results.Ok(response);
            }

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.NoContent;
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            return APIResponse.ReturnError(response, ex, Log.Logger);
        }
    }

    public virtual async Task<IResult> GetByIdAsync(Guid id)
    {
        Log.Logger.Information($"{RepositoryName}. Получение сущности с id='{id})'");
        var response = new APIResponse();
        try
        {
            if (!Guid.Empty.Equals(id))
            {
                var item = await repository.GetByIdAsync(id);
                if (item is not null)
                {
                    response.IsSuccess = true;
                    response.StatusCode = HttpStatusCode.OK;
                    response.Result = item;
                    return Results.Ok(response);
                }
            }

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.NoContent;
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            return APIResponse.ReturnError(response, ex, Log.Logger);
        }
    }

    public virtual async Task<IResult> GetAllAsync()
    {
        Log.Logger.Information($"{RepositoryName}. Получение всех записей");
        var response = new APIResponse();
        try
        {
            var data = await repository.GetAllAsync();
            if (!data.Any())
            {
                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.NoContent;
                return Results.NoContent();
            }

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Result = data;
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            return APIResponse.ReturnError(response, ex, Log.Logger);
        }
    }
}
