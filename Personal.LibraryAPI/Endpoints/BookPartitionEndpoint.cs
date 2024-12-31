using System.Net;
using Microsoft.AspNetCore.Mvc;
using Personal.Domain.Entities;
using Personal.Services.Response;
using Personal.Services.Services;
using Serilog;

namespace Personal.LibraryAPI.Endpoints;

public static class BookPartitionEndpoint
{
    public static void ConfigureBookPartitionEndpoints(this WebApplication app)
    {
        var BookPartitionMap = app.MapGroup("/api/BookPartition");
        BookPartitionMap.MapGet("/all", GetBookPartitions).WithName("GetAllBookPartitions").Produces<APIResponse>()
            .Produces(204).Produces(400);
        BookPartitionMap.MapGet("/{id:guid}", GetBookPartition).WithName("GetBookPartition").Produces<APIResponse>()
            .Produces(204).Produces(400);
        BookPartitionMap.MapPost("/", AddBookPartition).WithName("AddBookPartition");
        BookPartitionMap.MapPut("/", UpdateBookPartition).WithName("UpdateBookPartition");
        BookPartitionMap.MapDelete("/{id:guid}", DeleteBookPartition).WithName("DeleteBookPartition");
    }

    private static async Task<IResult> UpdateBookPartition([FromBody] BookPartition item_dto,
        IBookPartitionsService service)
    {
        Log.Logger.Information($"Обновление раздела: {item_dto.Name}('{item_dto._id}')");
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

    private static async Task<IResult> AddBookPartition([FromBody] BookPartition item_dto,
        IBookPartitionsService service)
    {
        Log.Logger.Information($"Добавление раздела: {item_dto.Name}");
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

    private static async Task<IResult> GetBookPartitions(IBookPartitionsService service)
    {
        Log.Logger.Information("Получение списка авторов");
        return await service.GetAllAsync();
    }

    private static async Task<IResult> GetBookPartition(Guid id, IBookPartitionsService service)
    {
        Log.Logger.Information($"Получение раздела id='{id}'");
        return await service.GetByIdAsync(id);
    }

    private static async Task<IResult> DeleteBookPartition(Guid id, IBookPartitionsService service)
    {
        Log.Logger.Information($"Удаление раздела id='{id}'");
        return await service.DeleteAsync(id);
    }
}
