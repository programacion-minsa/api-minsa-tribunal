using webApiTribunal.Models.Responses;

namespace webApiTribunal.Repositories.Interfaces;

public interface ITribunalService
{
    public Task<ResponseModel<FindPersonModel>> GetPatientById(string id);
    public Task<ResponseModel<bool>> StoreUserPetitionData(string id, bool responseStatus, string responseMessage);
}