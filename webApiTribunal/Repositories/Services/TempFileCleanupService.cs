using Microsoft.Extensions.Options;
using webApiTribunal.Models.Responses;

namespace webApiTribunal.Repositories.Services;

public class TempFileCleanupService: BackgroundService
{
    private readonly string _filesPath;

    public TempFileCleanupService(IOptions<FileSettings> settings)
    {
        _filesPath = settings.Value.FilesPath;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            CleanOldFiles();
        }
    }
    
    private void CleanOldFiles()
    {
        try
        {
            if (!Directory.Exists(_filesPath))
            {
                return;
            }

            var files = Directory.GetFiles(_filesPath);
            //var deleted = 0;

            foreach (var file in files)
            {
                var createdAt = File.GetCreationTimeUtc(file);
                var age = DateTime.UtcNow - createdAt;

                if (age.TotalHours >= 6)
                {
                    File.Delete(file);
                    //deleted++;
                }
            }

            // if (deleted > 0)
            //     _logger.LogInformation("Limpieza completada: {Count} archivo(s) eliminado(s).", deleted);deleted
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex, "Error durante la limpieza de archivos temporales.");
        }
    }
}