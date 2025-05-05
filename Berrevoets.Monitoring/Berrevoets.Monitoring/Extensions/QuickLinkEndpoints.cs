// Berrevoets.Monitoring/Berrevoets.Monitoring/Extensions/QuickLinkEndpoints.cs

using Berrevoets.Monitoring.Entities;
using Berrevoets.Monitoring.Entities.DTO;
using Microsoft.EntityFrameworkCore;

namespace Berrevoets.Monitoring.Extensions;

public static class QuickLinkEndpoints
{
    public static void MapQuickLinkEndpoints(this WebApplication app)
    {
        app.MapGet("/api/quicklinks", GetAllQuickLinks);
        app.MapGet("/api/quicklinks/{id}", GetQuickLinkById);
        app.MapPost("/api/quicklinks", CreateQuickLink);
        app.MapPut("/api/quicklinks/{id}", UpdateQuickLink);
        app.MapDelete("/api/quicklinks/{id}", DeleteQuickLink);
    }

    private static async Task<IResult> GetAllQuickLinks(HealthConfigDbContext db)
    {
        var quickLinks = await db.QuickLinks
            .Select(link => new QuickLinkDto
            {
                Id = link.Id,
                Title = link.Title,
                Url = link.Url,
                ButtonClass = link.ButtonClass,
                OrderNumber = link.OrderNumber,
                IsActive = link.IsActive,
                Icon = link.Icon
            })
            .OrderBy(link => link.OrderNumber)
            .ToListAsync();

        return Results.Ok(quickLinks);
    }

    private static async Task<IResult> GetQuickLinkById(Guid id, HealthConfigDbContext db)
    {
        var quickLink = await db.QuickLinks
            .Where(l => l.Id == id)
            .Select(link => new QuickLinkDto
            {
                Id = link.Id,
                Title = link.Title,
                Url = link.Url,
                ButtonClass = link.ButtonClass,
                OrderNumber = link.OrderNumber,
                IsActive = link.IsActive,
                Icon = link.Icon
            })
            .FirstOrDefaultAsync();

        if (quickLink == null)
            return Results.NotFound();

        return Results.Ok(quickLink);
    }

    private static async Task<IResult> CreateQuickLink(QuickLinkDto quickLinkDto, HealthConfigDbContext db)
    {
        var quickLink = new QuickLink
        {
            Id = quickLinkDto.Id,
            Title = quickLinkDto.Title,
            Url = quickLinkDto.Url,
            ButtonClass = quickLinkDto.ButtonClass,
            OrderNumber = quickLinkDto.OrderNumber,
            IsActive = quickLinkDto.IsActive,
            Icon = quickLinkDto.Icon
        };

        db.QuickLinks.Add(quickLink);
        await db.SaveChangesAsync();

        return Results.Created($"/api/quicklinks/{quickLink.Id}", quickLink);
    }

    private static async Task<IResult> UpdateQuickLink(Guid id, QuickLinkDto quickLinkDto, HealthConfigDbContext db)
    {
        var quickLink = await db.QuickLinks.FindAsync(id);
        if (quickLink == null)
            return Results.NotFound();

        // Update properties
        quickLink.Title = quickLinkDto.Title;
        quickLink.Url = quickLinkDto.Url;
        quickLink.ButtonClass = quickLinkDto.ButtonClass;
        quickLink.OrderNumber = quickLinkDto.OrderNumber;
        quickLink.IsActive = quickLinkDto.IsActive;
        quickLink.Icon = quickLinkDto.Icon;

        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteQuickLink(Guid id, HealthConfigDbContext db)
    {
        var quickLink = await db.QuickLinks.FindAsync(id);
        if (quickLink == null)
            return Results.NotFound();

        db.QuickLinks.Remove(quickLink);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
