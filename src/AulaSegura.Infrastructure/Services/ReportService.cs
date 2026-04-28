using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AulaSegura.Core.Entities;
using AulaSegura.Core.Interfaces;
using AulaSegura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AulaSegura.Infrastructure.Services
{
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
            var reports = await GetReportsByDateRangeAsync(startDate, endDate);
            
            var summaryReport = new Report
            {
                Timestamp = DateTime.UtcNow,
                ReportType = reportType,
                StartDate = startDate,
                EndDate = endDate,
                TotalBlocks = reports.Count(r => r.ActionTaken == "Blocked"),
                TotalViolations = reports.Count(),
                TopBlockedSite = reports
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
            // Implementación básica - en producción se usaría una librería CSV
            var csvContent = $"Fecha,Tipo,Bloqueos,Violaciones,Sitio Top\n";
            csvContent += $"{report.Timestamp:yyyy-MM-dd},{report.ReportType},{report.TotalBlocks},{report.TotalViolations},{report.TopBlockedSite}\n";
            
            var filePath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), fileName);
            await System.IO.File.WriteAllTextAsync(filePath, csvContent);
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

            if (oldReports.Any())
            {
                _context.Reports.RemoveRange(oldReports);
                await _context.SaveChangesAsync();
            }
        }
    }
}
