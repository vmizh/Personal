using Microsoft.AspNetCore.Mvc;
using Personal.Domain.Entities;
using Personal.Services.Response;
using Personal.Services.Services;
using Serilog;
using System.Net;

namespace Personal.LibraryAPI.Endpoints;

public static class BookEndpoint
{
    public static void ConfigureBookEndpoints(this WebApplication app)
    {
        var bookMap = app.MapGroup("/api/book");
        bookMap.MapGet("/all", GetBooks).WithName("GetAllBooks").Produces<APIResponse>()
            .Produces(204).Produces(400);
        bookMap.MapGet("/{id:guid}", GetBook).WithName("GetBook").Produces<APIResponse>()
            .Produces(204).Produces(400);
        bookMap.MapPost("/", AddBook).WithName("AddBook");
        bookMap.MapPut("/", UpdateBook).WithName("UpdateBook");
        bookMap.MapDelete("/{id:guid}", DeleteBook).WithName("DeleteBook");
    }

    private static async Task<IResult> UpdateBook([FromBody] Book item_dto,
        IBookService service)
    {
        Log.Logger.Information($"Обновление униги: {item_dto.Name}('{item_dto._id}')");
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

    private static async Task<IResult> DeleteBook(Guid id, IBookService service)
    {
        Log.Logger.Information($"Удаление книги id='{id}'");
        return await service.DeleteAsync(id);
    }

    private static async Task<IResult> AddBook([FromBody] Book item_dto,
        IBookService service)
    {
        Log.Logger.Information($"Добавление книги: {item_dto.Name}");
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

    private static async Task<IResult> GetBooks(IBookService service)
    {
        Log.Logger.Information("Получение списка книг");
        return await service.GetAllAsync();
    }

    private static async Task<IResult> GetBook(Guid id, IBookService service)
    {
        Log.Logger.Information($"Получение книги id='{id}'");
        return await service.GetByIdAsync(id);
    }
}
