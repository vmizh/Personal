using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using ILogger = Serilog.ILogger;

namespace Personal.Services.Response;

public class APIResponse
{
    public bool IsSuccess { get; set; }
    public object? Result { get; set; }

    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.BadRequest;

    public List<string> ErrorMessages { set; get; } = new List<string>();

    public static IResult ReturnError(APIResponse response, Exception ex, ILogger _logger,
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
    {
        response.IsSuccess = false;
        response.StatusCode = statusCode;
        var ex1 = ex;
        var errText = new StringBuilder("------------------------------\n");
        while (ex1 != null)
        {
            errText.Append($"{ex1.Message}\n");
            response.ErrorMessages.Add(ex1.Message);
            ex1 = ex1.InnerException;
        }

        errText.Append("------------------------------");
        _logger.Error(errText.ToString());
        return Results.BadRequest(response);
    }

    
}
