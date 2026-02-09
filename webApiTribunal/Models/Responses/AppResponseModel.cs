namespace webApiTribunal.Models.Responses;

public class AppResponseModel
{
    public string AppName { get; set; } = "";
    public string AppDescription { get; set; } = "";
    public string AppToken { get; set; } = "";
    public bool IsActive { get; set; } = false;
    public DateTime? CreatedAt { get; set; }
}