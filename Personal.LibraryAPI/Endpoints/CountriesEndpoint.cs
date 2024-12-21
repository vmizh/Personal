using Microsoft.AspNetCore.Mvc;
using Personal.Services.Response;
using Serilog;
using System.Net;
using Personal.Domain.Entities;
using Personal.Services.Services;

namespace Personal.LibraryAPI.Endpoints;

public static class CountriesEndpoint
{
    public static void ConfigureCountriesEndpoints(this WebApplication app)
    {
        var userMap = app.MapGroup("/api/country");
        userMap.MapGet("/all", GetCountries).WithName("GetAllCountries").Produces<APIResponse>()
            .Produces(204).Produces(400);
        userMap.MapGet("/{id:guid}", GetCountry).WithName("GetCountry").Produces<APIResponse>()
            .Produces(204).Produces(400);
        userMap.MapPost("/", AddCountry).WithName("AddCountry");
    }

    private static async Task<IResult> AddCountry([FromBody] Country item_dto,
        ICountriesService service)
    {
        Log.Logger.Information($"Добавление страны: {item_dto.Name}");
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

    private static async Task<IResult> GetCountries(ICountriesService service)
    {
        Log.Logger.Information("Получение списка стран");
        return await service.GetAllAsync();
    }

    private static async Task<IResult> GetCountry(Guid id, ICountriesService service)
    {
        Log.Logger.Information($"Получение страны id='{id}'");
        return await service.GetByIdAsync(id);
    }
}
