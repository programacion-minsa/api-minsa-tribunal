using System.Net.Http.Headers;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using webApiTribunal.Middlewares;
using webApiTribunal.Models.Entities;
using webApiTribunal.Repositories.Interfaces;
using webApiTribunal.Repositories.Services;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    
    // Only use these two lines if your Nginx and API are in a secure/private network.
    // It clears the default 'KnownProxies' list to accept headers from any IP.
    // options.KnownProxies.Clear(); 
    // options.KnownNetworks.Clear();
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<DBContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDbContextFactory<DBContext>(options => options.UseSqlServer(connectionString),
    ServiceLifetime.Transient);

// HttpClient Configuration
builder.Services.AddHttpClient("HttpClientApiTribunalElectoral", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiTribunalData:API_URL"] ?? "");
    client.DefaultRequestHeaders.Add("Accept", "text/xml");
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    string thumbprint = builder.Configuration["ApiTribunalData:CERT_THUMBPRINT"] ?? "";
    string password = builder.Configuration["ApiTribunalData:CERT_PASSWORD"] ?? "";
    string localCertPath = builder.Configuration["ApiTribunalData:CERT_PATH"] ?? "";
    
    var certificate = LoadCertificateClass.LoadCertificateFromStore(thumbprint, password, localCertPath);
    var handler = new HttpClientHandler
    {
        ClientCertificateOptions = ClientCertificateOption.Manual
    };

    handler.ClientCertificates.Add(certificate);
    handler.ClientCertificateOptions = ClientCertificateOption.Manual;

    return handler;
});

builder.Services.AddRateLimiter(options =>
{
    // Configure a named policy (e.g., "FixedPolicy")
    options.AddFixedWindowLimiter("FixedPolicy", opt =>
    {
        opt.PermitLimit = 100; // Allow 100 requests
        opt.Window = TimeSpan.FromMinutes(1); // within a 1-minute window
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 5; // Queue up to 5 requests if the limit is exceeded
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IAccessService, AccessService>();
builder.Services.AddTransient<ITribunalService, TribunalService>();

var app = builder.Build();
app.UseRateLimiter();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// app.UseForwardedHeaders(new ForwardedHeadersOptions
// {
//     ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
//     
//     options.KnownProxies.Clear(); 
//     options.KnownNetworks.Clear();
// });

app.UseExceptionHandler("/error");

app.UseMiddleware<ApiKeyValidatorMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/error", (HttpContext context) =>
{
    var exceptionFeature = context.Features.Get<IExceptionHandlerPathFeature>();
    var exception = exceptionFeature?.Error;

    // Log the full exception details here
    context.RequestServices.GetRequiredService<ILogger<Program>>()
        .LogError(exception, "Unhandled exception encountered.");

    // Return a generic 500 status code to the client
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    return new
    {
        Title = "An error occurred.",
        Status = 500,
        Detail = "Internal configuration or service error. Check server logs."
    };
});

app.Run();