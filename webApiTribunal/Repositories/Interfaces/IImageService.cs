using webApiTribunal.Models.Responses;

namespace webApiTribunal.Repositories.Interfaces;

public interface IImageService
{
   Task<ResutlModel<string>> DownloadToTempAsync(string imageUrl, CancellationToken ct = default);
}