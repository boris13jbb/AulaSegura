using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AulaSegura.Core.Entities;

namespace AulaSegura.Core.Interfaces
{
    public interface IReportService
    {
        Task<IEnumerable<Report>> GetReportsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<Report> CreateReportAsync(Report report);
        Task<Report> GenerateReportAsync(DateTime startDate, DateTime endDate, ReportType reportType);
        Task<IEnumerable<Report>> GetReportsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task ExportReportToCsvAsync(Report report, string fileName);
        Task<long> GetTotalBlockedCountAsync();
        Task<IEnumerable<Report>> GetRecentReportsAsync(int count = 50);
        Task DeleteOldReportsAsync(int daysToKeep = 90);
    }
}
