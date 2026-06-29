using AulaSegura.Core.Entities;
using AulaSegura.Core.Interfaces;
using AulaSegura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace AulaSegura.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly AulaSeguraDbContext _context;

    public ReportService(AulaSeguraDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Report>> GetReportsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Reports.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(r => r.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(r => r.Timestamp <= endDate.Value);

        return await query
            .OrderByDescending(r => r.Timestamp)
            .ToListAsync();
    }

    public async Task<Report> CreateReportAsync(Report report)
    {
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();
        return report;
    }

    public async Task<Report> GenerateReportAsync(DateTime startDate, DateTime endDate, ReportType reportType)
    {
        var reports = (await GetReportsByDateRangeAsync(startDate, endDate)).ToList();

        var summaryReport = new Report
        {
            Timestamp = DateTime.UtcNow,
            ReportType = reportType,
            StartDate = startDate,
            EndDate = endDate,
            TotalBlocks = reports.Count(r => r.ActionTaken == "Blocked"),
            TotalViolations = reports.Count,
            TopBlockedSite = reports
                .Where(r => !string.IsNullOrWhiteSpace(r.Url))
                .GroupBy(r => r.Url)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key ?? "N/A"
        };

        _context.Reports.Add(summaryReport);
        await _context.SaveChangesAsync();
        return summaryReport;
    }

    public async Task<IEnumerable<Report>> GetReportsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Reports
            .Where(r => r.Timestamp >= startDate && r.Timestamp <= endDate)
            .OrderByDescending(r => r.Timestamp)
            .ToListAsync();
    }

    public async Task ExportReportToCsvAsync(Report report, string fileName)
    {
        var filePath = Path.IsPathRooted(fileName)
            ? fileName
            : Path.Combine(Directory.GetCurrentDirectory(), fileName);

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var csvContent = "Fecha,Tipo,Bloqueos,Violaciones,Sitio Top\n";
        csvContent += string.Join(
            ',',
            new[]
            {
                EscapeCsv(report.Timestamp.ToString("yyyy-MM-dd")),
                EscapeCsv(report.ReportType.ToString()),
                report.TotalBlocks.ToString(CultureInfo.InvariantCulture),
                report.TotalViolations.ToString(CultureInfo.InvariantCulture),
                EscapeCsv(report.TopBlockedSite ?? string.Empty)
            });
        csvContent += "\n";

        await File.WriteAllTextAsync(filePath, csvContent);
    }

    public async Task<long> GetTotalBlockedCountAsync()
    {
        return await _context.Reports
            .CountAsync(r => r.ActionTaken == "Blocked");
    }

    public async Task<IEnumerable<Report>> GetRecentReportsAsync(int count = 50)
    {
        return await _context.Reports
            .OrderByDescending(r => r.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task DeleteOldReportsAsync(int daysToKeep = 90)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
        var oldReports = await _context.Reports
            .Where(r => r.Timestamp < cutoffDate)
            .ToListAsync();

        if (!oldReports.Any())
            return;

        _context.Reports.RemoveRange(oldReports);
        await _context.SaveChangesAsync();
    }

    private static string EscapeCsv(string value)
    {
        if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\n') && !value.Contains('\r'))
            return value;

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
