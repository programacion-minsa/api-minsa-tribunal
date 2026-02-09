using webApiTribunal.Repositories.Interfaces;

namespace webApiTribunal.Middlewares;

public class ApiKeyValidatorMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeaderName = "X-Api-Key";
    private readonly List<string> _restrictedPaths;

    public ApiKeyValidatorMiddleware(RequestDelegate next)
    {
        _next = next;
        _restrictedPaths = new List<string>
        {
            "/api/tribunal/index",
            "/api/tribunal/findbyid",
        };
    }

    public async Task InvokeAsync(HttpContext context, IAccessService AccessService)
    {
        var requestPath = context.Request.Path.Value;
        if (_restrictedPaths.Any(p => requestPath.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("debe proporcionar el api key.");
                return;
            }

            var data = await AccessService.ValidateApiKey(extractedApiKey);
            if (!data.Success)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync(data.Message);
                return;
            }
        }

        await _next(context);
    }
}