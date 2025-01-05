using System.Net;
using Microsoft.AspNetCore.Mvc;
using Personal.Domain.Entities;
using Personal.Services.Response;
using Personal.Services.Services;
using Serilog;

namespace Personal.LibraryAPI.Endpoints;

public static class GenreEndpoint
{
    public static void ConfigureGenreEndpoints(this WebApplication app)
    {
        var authorMap = app.MapGroup("/api/genre");
        authorMap.MapGet("/all", GetGenres).WithName("GetAllGenres").Produces<APIResponse>()
            .Produces(204).Produces(400);
        authorMap.MapGet("/{id:guid}", GetGenre).WithName("GetGenre").Produces<APIResponse>()
            .Produces(204).Produces(400);
        authorMap.MapPost("/", AddGenre).WithName("AddGenre");
        authorMap.MapPut("/", UpdateGenre).WithName("UpdateGenre");
        authorMap.MapDelete("/{id:guid}", DeleteGenre).WithName("DeleteGenre");
    }

    private static async Task<IResult> UpdateGenre([FromBody] Genre item_dto,
        IGenreService service)
    {
        Log.Logger.Information($"Обновление типа литературы: {item_dto.Name}('{item_dto._id}')");
        var response = new APIResponse();
        try
        {
            var saved = await service.UpdateAsync(item_dto);
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

    private static async Task<IResult> AddGenre([FromBody] Genre item_dto,
        IGenreService service)
    {
        Log.Logger.Information($"Добавление типа литературы: {item_dto.Name}");
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

    private static async Task<IResult> GetGenres(IGenreService service)
    {
        Log.Logger.Information("Получение списка типов литературы");
        return await service.GetAllAsync();
    }

    private static async Task<IResult> GetGenre(Guid id, IGenreService service)
    {
        Log.Logger.Information($"Получение типа литературы id='{id}'");
        return await service.GetByIdAsync(id);
    }

    private static async Task<IResult> DeleteGenre(Guid id, IGenreService service)
    {
        Log.Logger.Information($"Удаление типа литературы id='{id}'");
        return await service.DeleteAsync(id);
    }
}
