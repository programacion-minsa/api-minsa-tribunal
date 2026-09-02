using System;
using System.Collections.Generic;

namespace webApiTribunal.Models.Entities;

public partial class AppsAccessLog
{
    public int Id { get; set; }

    public string? UserAgent { get; set; }

    public string? UserIpAddress { get; set; }

    public int? AppId { get; set; }

    public string? AppApiKey { get; set; }

    public string? AppName { get; set; }

    public DateTime? RequestDate { get; set; }

    public bool? ResponseStatus { get; set; }

    public string? ResponseMessage { get; set; }

    public string? RequestId { get; set; }
}
