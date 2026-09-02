using Microsoft.Extensions.Options;
using webApiTribunal.Models.Responses;

namespace webApiTribunal.Repositories.Interfaces;

public class ImageService : IImageService
{
    private readonly HttpClient _http;
    private readonly string _filesPath;

    public ImageService(HttpClient http, IOptions<FileSettings> settings)
    {
        _http = http;
        _filesPath = settings.Value.FilesPath;

        // Crea la carpeta si no existe
        Directory.CreateDirectory(_filesPath);
    }

    public async Task<ResutlModel<string>> DownloadToTempAsync(string imageUrl, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync(imageUrl, ct);
            response.EnsureSuccessStatusCode();

            var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
            var ext = contentType switch
            {
                "image/png"  => ".png",
                "image/gif"  => ".gif",
                "image/webp" => ".webp",
                _            => ".jpg"
            };

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(_filesPath, fileName);
            if (File.Exists(filePath))
            {
                fileName = $"{Guid.NewGuid()}{ext}";
                filePath = Path.Combine(_filesPath, fileName);
            }

            await using var fs = File.Create(filePath);
            await response.Content.CopyToAsync(fs, ct);

            return new ResutlModel<string>
            {
                Success = true,
                Message = "Imagen descargada correctamente.",
                Data = fileName
            };
        }
        catch (HttpRequestException ex)
        {
            return new ResutlModel<string>
            {
                Success = false,
                Message = $"Error al descargar la imagen desde la URL: {ex.Message}"
            };
        }
        catch (IOException ex)
        {
            return new ResutlModel<string>
            {
                Success = false,
                Message = $"Error al guardar el archivo en disco: {ex.Message}"
            };
        }
        catch (OperationCanceledException)
        {
            return new ResutlModel<string>
            {
                Success = false,
                Message = "La operación fue cancelada."
            };
        }
        catch (Exception ex)
        {
            return new ResutlModel<string>
            {
                Success = false,
                Message = $"Error inesperado: {ex.Message}"
            };
        }
    }
}