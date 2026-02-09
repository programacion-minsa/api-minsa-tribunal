using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using webApiTribunal.Models.Entities;
using webApiTribunal.Models.Forms;
using webApiTribunal.Models.Responses;
using webApiTribunal.Repositories.Interfaces;

namespace webApiTribunal.Repositories.Services;

public class AccessService(IDbContextFactory<DBContext> context) : IAccessService
{
    #region PUBLIC METHODS

    public async Task<ResponseModel<AppResponseModel>> CreateAppAccessToken(AppModel appModel)
    {
        await using var localContext = await context.CreateDbContextAsync();
        AppsAccessToken newApp = new AppsAccessToken()
        {
            AppName = appModel.AppName,
            AppDescription = appModel.AppDescription,
            AppToken = TokenGenerator(),
            CreatedAt = DateTime.Now,
            IsActive = true
        };
        localContext.AppsAccessToken.Add(newApp);

        try
        {
            await localContext.SaveChangesAsync();

            var responseData = new AppResponseModel()
            {
                AppName = newApp.AppName,
                AppDescription = newApp.AppDescription,
                AppToken = newApp.AppToken,
                IsActive = (bool)newApp.IsActive,
                CreatedAt = newApp.CreatedAt,
            };

            return new ResponseModel<AppResponseModel>()
            {
                Success = true,
                Message = "token created.",
                Data = responseData
            };
        }
        catch (Exception ex)
        {
            return new ResponseModel<AppResponseModel>()
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ResponseModel<bool>> ValidateApiKey(string ApiKey)
    {
        await using var localContext = await context.CreateDbContextAsync();
        var result = await localContext.AppsAccessToken.Where(x => x.AppToken == ApiKey).FirstOrDefaultAsync();

        if (result == null)
        {
            return new ResponseModel<bool>()
            {
                Success = false,
                Message = "la api key proporcionada no esta registrada en la base de datos."
            };
        }

        if ((bool)(!result.IsActive)!)
        {
            return new ResponseModel<bool>()
            {
                Success = false,
                Message = "la api key proporcionada ya no tiene permiso para acceder al api."
            };
        }

        return new ResponseModel<bool>()
        {
            Success = true,
            Message = ""
        };
    }

    #endregion

    #region PRIVATE METHODS

    private string TokenGenerator()
    {
        int lengthInBytes = 32;
        byte[] keyBytes = new byte[lengthInBytes];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(keyBytes);
        }

        string Token = Convert.ToBase64String(keyBytes);
        return Token;
    }

    #endregion
}