namespace Berrevoets.Monitoring;

public static class Helpers
{
    // Helper method to determine status priority for sorting
    public static int GetStatusPriority(string status)
    {
        return status switch
        {
            "Unhealthy" => 0,
            "Error" => 1,
            "Degraded" => 2,
            "Healthy" => 3,
            _ => 4
        };
    }
}
