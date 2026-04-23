using Finance.Application.Common.Interfaces.Reports;
using Finance.Application.Features.Reports.Dtos;
using Finance.Application.Features.Reports.GetBalanceSheet;
using Finance.Application.Features.Reports.GetCashFlow;
using Finance.Application.Features.Reports.GetExecutiveSummary;
using Finance.Application.Features.Reports.GetIncomeStatement;
using Finance.Application.Features.Reports.GetTrialBalance;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Finance.API.Controllers.Reports;

[ApiController]
[Route("api/reports")]
public class ReportsController(
    ISender sender,
    IReportExportService reportExportService,
    IConfiguration configuration,
    TimeProvider timeProvider) : ControllerBase
{
    private const string DefaultCompanyName = "Finance System";

    [HttpGet("executive-summary")]
    [ProducesResponseType(typeof(ExecutiveSummaryReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ExecutiveSummaryReportDto>> GetExecutiveSummary(
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetExecutiveSummaryQuery(fromDate, toDate), cancellationToken);
        return Ok(result);
    }

    [HttpGet("trial-balance")]
    [ProducesResponseType(typeof(TrialBalanceReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TrialBalanceReportDto>> GetTrialBalance(
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTrialBalanceQuery(fromDate, toDate), cancellationToken);
        return Ok(result);
    }

    [HttpGet("trial-balance/export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportTrialBalance(
        [FromQuery] string? format,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] string? companyName,
        CancellationToken cancellationToken)
    {
        if (!TryParseExportFormat(format, out var exportFormat))
        {
            return InvalidFormat();
        }

        var report = await sender.Send(new GetTrialBalanceQuery(fromDate, toDate), cancellationToken);
        var file = await reportExportService.ExportTrialBalanceAsync(
            report,
            exportFormat,
            ResolveCompanyName(companyName),
            timeProvider.GetUtcNow().UtcDateTime,
            cancellationToken);

        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("income-statement")]
    [ProducesResponseType(typeof(IncomeStatementReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IncomeStatementReportDto>> GetIncomeStatement(
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetIncomeStatementQuery(fromDate, toDate), cancellationToken);
        return Ok(result);
    }

    [HttpGet("income-statement/export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportIncomeStatement(
        [FromQuery] string? format,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] string? companyName,
        CancellationToken cancellationToken)
    {
        if (!TryParseExportFormat(format, out var exportFormat))
        {
            return InvalidFormat();
        }

        var report = await sender.Send(new GetIncomeStatementQuery(fromDate, toDate), cancellationToken);
        var file = await reportExportService.ExportIncomeStatementAsync(
            report,
            exportFormat,
            ResolveCompanyName(companyName),
            timeProvider.GetUtcNow().UtcDateTime,
            cancellationToken);

        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("balance-sheet")]
    [ProducesResponseType(typeof(BalanceSheetReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<BalanceSheetReportDto>> GetBalanceSheet(
        [FromQuery] DateOnly? asOfDate,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBalanceSheetQuery(asOfDate), cancellationToken);
        return Ok(result);
    }

    [HttpGet("balance-sheet/export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportBalanceSheet(
        [FromQuery] string? format,
        [FromQuery] DateOnly? asOfDate,
        [FromQuery] string? companyName,
        CancellationToken cancellationToken)
    {
        if (!TryParseExportFormat(format, out var exportFormat))
        {
            return InvalidFormat();
        }

        var report = await sender.Send(new GetBalanceSheetQuery(asOfDate), cancellationToken);
        var file = await reportExportService.ExportBalanceSheetAsync(
            report,
            exportFormat,
            ResolveCompanyName(companyName),
            timeProvider.GetUtcNow().UtcDateTime,
            cancellationToken);

        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("executive-summary/export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportExecutiveSummary(
        [FromQuery] string? format,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] string? companyName,
        CancellationToken cancellationToken)
    {
        if (!TryParseExportFormat(format, out var exportFormat))
        {
            return InvalidFormat();
        }

        var report = await sender.Send(new GetExecutiveSummaryQuery(fromDate, toDate), cancellationToken);
        var file = await reportExportService.ExportExecutiveSummaryAsync(
            report,
            exportFormat,
            ResolveCompanyName(companyName),
            timeProvider.GetUtcNow().UtcDateTime,
            cancellationToken);

        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("cash-flow")]
    [ProducesResponseType(typeof(CashFlowSummaryReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CashFlowSummaryReportDto>> GetCashFlow(
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCashFlowQuery(fromDate, toDate), cancellationToken);
        return Ok(result);
    }

    [HttpGet("cash-flow/export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportCashFlow(
        [FromQuery] string? format,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] string? companyName,
        CancellationToken cancellationToken)
    {
        if (!TryParseExportFormat(format, out var exportFormat))
        {
            return InvalidFormat();
        }

        var report = await sender.Send(new GetCashFlowQuery(fromDate, toDate), cancellationToken);
        var file = await reportExportService.ExportCashFlowAsync(
            report,
            exportFormat,
            ResolveCompanyName(companyName),
            timeProvider.GetUtcNow().UtcDateTime,
            cancellationToken);

        return File(file.Content, file.ContentType, file.FileName);
    }

    private string ResolveCompanyName(string? companyName)
    {
        if (!string.IsNullOrWhiteSpace(companyName))
        {
            return companyName.Trim();
        }

        return configuration["Company:Name"] ?? DefaultCompanyName;
    }

    private static bool TryParseExportFormat(string? format, out ReportExportFormat exportFormat)
    {
        exportFormat = default;

        if (string.IsNullOrWhiteSpace(format))
        {
            return false;
        }

        if (format.Equals("pdf", StringComparison.OrdinalIgnoreCase))
        {
            exportFormat = ReportExportFormat.Pdf;
            return true;
        }

        if (format.Equals("excel", StringComparison.OrdinalIgnoreCase) ||
            format.Equals("xlsx", StringComparison.OrdinalIgnoreCase))
        {
            exportFormat = ReportExportFormat.Excel;
            return true;
        }

        return false;
    }

    private static BadRequestObjectResult InvalidFormat()
    {
        return new BadRequestObjectResult(new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Invalid export format",
            Detail = "Supported report export formats are pdf and excel."
        });
    }
}
