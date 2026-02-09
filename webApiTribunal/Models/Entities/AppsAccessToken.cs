using System;
using System.Collections.Generic;

namespace webApiTribunal.Models.Entities;

public partial class AppsAccessToken
{
    public int Id { get; set; }

    public string? AppName { get; set; }

    public string? AppDescription { get; set; }

    public string AppToken { get; set; } = null!;

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
