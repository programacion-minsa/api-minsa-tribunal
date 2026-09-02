namespace webApiTribunal.Models.Responses;

public class FindPersonModel
{
    public PersonModel? Person { get; set; } = new();
    public MessageModel? Message { get; set; } = new();
    public CedulaImageModel? CedulaImagenes { get; set; } = new();
}