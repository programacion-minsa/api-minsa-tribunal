using webApiTribunal.Models.Forms;
using webApiTribunal.Models.Responses;

namespace webApiTribunal.Repositories.Interfaces;

public interface IAccessService
{
    public Task<ResponseModel<AppResponseModel>> CreateAppAccessToken(AppModel appModel);
    public Task<ResponseModel<bool>> ValidateApiKey(string ApiKey);
}