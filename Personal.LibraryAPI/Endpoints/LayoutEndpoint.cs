using Microsoft.AspNetCore.Mvc;
using Personal.Domain.Entities;
using Personal.Services.Response;
using Personal.Services.Services;
using Serilog;
using System.Net;

namespace Personal.LibraryAPI.Endpoints;


public static class LayoutEndpoint
{

    public static void ConfigureLayoutEndpoints(this WebApplication app)
    {
        var lauyoutMap = app.MapGroup("/api/layout");
        lauyoutMap.MapGet("/all", GetLayouts).WithName("GetAllLayouts").Produces<APIResponse>()
            .Produces(204).Produces(400);
        lauyoutMap.MapGet("/{id:guid}", GetLayout).WithName("GetLayout").Produces<APIResponse>()
            .Produces(204).Produces(400);
        lauyoutMap.MapPost("/", AddLayout).WithName("AddLayout");
        lauyoutMap.MapPut("/", UpdateLayout).WithName("UpdateLayout");
        lauyoutMap.MapDelete("/{id:guid}", DeleteLayout).WithName("DeleteLayout");
    }

    private static async Task<IResult> DeleteLayout(Guid id, ILayoutService service)
    {
        Log.Logger.Information($"Удаление разметки id='{id}'");
        return await service.DeleteAsync(id);
    }

    private static async Task<IResult> UpdateLayout([FromBody] Layout item_dto,
        ILayoutService service)
    {
        Log.Logger.Information($"Обновление разметки: '{item_dto._id}'");
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

    private static async Task<IResult> AddLayout([FromBody] Layout item_dto,
        ILayoutService service)
    {
        Log.Logger.Information($"Добавление разметки: {item_dto._id}");
        var response = new APIResponse();
        try
        {
            await service.CreateAsync(item_dto);
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            return APIResponse.ReturnError(response, ex, Log.Logger);
        }
    }

    private static async Task<IResult> GetLayouts(ILayoutService service)
    {
        Log.Logger.Information("Получение списка разметки");
        return await service.GetAllAsync();
    }

    private static async Task<IResult> GetLayout(Guid id, ILayoutService service)
    {
        Log.Logger.Information($"Получение разметки id='{id}'");
        return await service.GetByIdAsync(id);
    }
}
