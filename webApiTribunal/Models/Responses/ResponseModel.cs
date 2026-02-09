namespace webApiTribunal.Models.Responses;

public class ResponseModel<T>
{
    public int StatusCode { get; set; } = 0;
    public bool Success { get; set; } = false;
    public string Message { get; set; } = "";
    public T? Data { get; set; }
}