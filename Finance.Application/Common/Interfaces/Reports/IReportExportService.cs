using Finance.Application.Features.Reports.Dtos;

namespace Finance.Application.Common.Interfaces.Reports;

public interface IReportExportService
{
    Task<ReportExportFileDto> ExportTrialBalanceAsync(
        TrialBalanceReportDto report,
        ReportExportFormat format,
        string companyName,
        DateTime generatedAt,
        CancellationToken cancellationToken = default);

    Task<ReportExportFileDto> ExportIncomeStatementAsync(
        IncomeStatementReportDto report,
        ReportExportFormat format,
        string companyName,
        DateTime generatedAt,
        CancellationToken cancellationToken = default);

    Task<ReportExportFileDto> ExportBalanceSheetAsync(
        BalanceSheetReportDto report,
        ReportExportFormat format,
        string companyName,
        DateTime generatedAt,
        CancellationToken cancellationToken = default);

    Task<ReportExportFileDto> ExportExecutiveSummaryAsync(
        ExecutiveSummaryReportDto report,
        ReportExportFormat format,
        string companyName,
        DateTime generatedAt,
        CancellationToken cancellationToken = default);

    Task<ReportExportFileDto> ExportCashFlowAsync(
        CashFlowSummaryReportDto report,
        ReportExportFormat format,
        string companyName,
        DateTime generatedAt,
        CancellationToken cancellationToken = default);
}
