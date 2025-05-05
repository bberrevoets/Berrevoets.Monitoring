namespace Berrevoets.Monitoring.Entities.DTO;

public class QuickLinkDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ButtonClass { get; set; } = "btn-primary";
    public int OrderNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Icon { get; set; }
}
