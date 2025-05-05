// Berrevoets.Monitoring.Entities/QuickLink.cs

using System.ComponentModel.DataAnnotations;

namespace Berrevoets.Monitoring.Entities;

public class QuickLink
{
    public Guid Id { get; set; }

    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2048)]
    public string Url { get; set; } = string.Empty;

    [MaxLength(50)]
    public string ButtonClass { get; set; } = "btn-primary";

    public int OrderNumber { get; set; }

    public bool IsActive { get; set; } = true;
    public string? Icon { get; set; }
}
