namespace webApiTribunal.Models.Responses;

public class ResutlModel<T>
{
    public bool Success { get; set; } = false;
    public string Message { get; set; } = "";
    public T? Data { get; set; }
}