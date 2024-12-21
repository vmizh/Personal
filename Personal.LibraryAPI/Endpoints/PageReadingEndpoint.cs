using Microsoft.AspNetCore.Mvc;
using Personal.Domain.Entities;
using Personal.Services.Response;
using Personal.Services.Services;
using Serilog;
using System.Net;

namespace Personal.LibraryAPI.Endpoints;

public static class PageReadingEndpoint
{
    public static void ConfigurePageReadingEndpoints(this WebApplication app)
    {
        var readPageMap = app.MapGroup("/api/pagereading");
        readPageMap.MapGet("/all", GetPageReadings).WithName("GetAllPageReadings").Produces<APIResponse>()
            .Produces(204).Produces(400);
        readPageMap.MapGet("/{id:guid}", GetPageReading).WithName("GetPageReading").Produces<APIResponse>()
            .Produces(204).Produces(400);
        readPageMap.MapPost("/", AddPageReading).WithName("AddPageReading");
        readPageMap.MapPut("/", UpdatePageReading).WithName("UpdatePageReading");
        readPageMap.MapDelete("/{id:guid}", DeletePageReading).WithName("DeletePageReading");
    }

    private static async Task<IResult> DeletePageReading(Guid id, IReadPagingService service)
    {
        Log.Logger.Information($"Удаление режима чтения id='{id}'");
        return await service.DeleteAsync(id);
    }
    private static async Task<IResult> UpdatePageReading([FromBody] ReadPaging item_dto,
        IReadPagingService service)
    {
        Log.Logger.Information($"Обновление режима чтения: {item_dto.Name}('{item_dto._id}')");
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

    private static async Task<IResult> AddPageReading([FromBody] ReadPaging item_dto,
        IReadPagingService service)
    {
        Log.Logger.Information($"Добавление книги для чтения: {item_dto.Name}");
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

    private static async Task<IResult> GetPageReadings(IReadPagingService service)
    {
        Log.Logger.Information("Получение списка книг");
        return await service.GetAllAsync();
    }

    private static async Task<IResult> GetPageReading(Guid id, IReadPagingService service)
    {
        Log.Logger.Information($"Получение книги id='{id}'");
        return await service.GetByIdAsync(id);
    }
}
