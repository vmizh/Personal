using System.Net;
using Microsoft.AspNetCore.Mvc;
using Personal.Domain.Entities;
using Personal.Services.Response;
using Personal.Services.Services;
using Serilog;
using ServiceStack.Web;

namespace Personal.LibraryAPI.Endpoints;

public static class AuthorsEndpoint
{
    public static void ConfigureAuthorEndpoints(this WebApplication app)
    {
        var authorMap = app.MapGroup("/api/author");
        authorMap.MapGet("/all", GetAuthors).WithName("GetAllAuthors").Produces<APIResponse>()
            .Produces(204).Produces(400);
        authorMap.MapGet("/{id:guid}", GetAuthor).WithName("GetAuthor").Produces<APIResponse>()
            .Produces(204).Produces(400);
        authorMap.MapPost("/", AddAuthor).WithName("AddAuthor");
        authorMap.MapPut("/", UpdateAuthor).WithName("UpdateAuthor");
        authorMap.MapDelete("/{id:guid}", DeleteAuthor).WithName("DeleteAuthor");
    }

    private static async Task<IResult> UpdateAuthor([FromBody] Author item_dto,
        IAuthorService service)
    {
        Log.Logger.Information($"Обновление автора: {item_dto.Name}('{item_dto._id}')");
        var response = new APIResponse();
        try
        {
            var saved= await service.UpdateAsync(item_dto);
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Result = saved;
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            return APIResponse.ReturnError(response, ex, Log.Logger);
        }
    }

    private static async Task<IResult> AddAuthor([FromBody] Author item_dto,
        IAuthorService service)
    {
        Log.Logger.Information($"Добавление автора: {item_dto.Name}");
        var response = new APIResponse();
        try
        {
            var ret = await service.CreateAsync(item_dto);
            
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Result = ret;
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            return APIResponse.ReturnError(response, ex, Log.Logger);
        }
    }

    private static async Task<IResult> GetAuthors(IAuthorService service)
    {
        Log.Logger.Information("Получение списка авторов");
        return await service.GetAllAsync();
    }

    private static async Task<IResult> GetAuthor(Guid id, IAuthorService service)
    {
        Log.Logger.Information($"Получение автора id='{id}'");
        return await service.GetByIdAsync(id);
    }

    private static async Task<IResult> DeleteAuthor(Guid id, IAuthorService service)
    {
        Log.Logger.Information($"Удаление автора id='{id}'");
        return await service.DeleteAsync(id);
    }
}
