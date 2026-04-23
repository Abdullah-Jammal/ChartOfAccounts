using Finance.Application.Common.Interfaces.Reports;
using Finance.Application.Features.Reports.Dtos;
using System.Globalization;
using System.IO.Compression;
using System.Security;
using System.Text;

namespace Finance.Infrastructure.Reports;

public class ReportExportService : IReportExportService
{
    private const string PdfContentType = "application/pdf";
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public Task<ReportExportFileDto> ExportTrialBalanceAsync(
        TrialBalanceReportDto report,
        ReportExportFormat format,
        string companyName,
        DateTime generatedAt,
        CancellationToken cancellationToken = default)
    {
        var rows = new List<IReadOnlyList<string>>
        {
            Row("Total debit", Money(report.TotalDebit), ""),
            Row("Total credit", "", Money(report.TotalCredit)),
            Row("Balanced", report.IsBalanced ? "Yes" : "No", ""),
            EmptyRow()
        };

        rows.AddRange(report.Accounts.Select(line => Row(
            $"{line.AccountCode} - {line.AccountName}",
            Money(line.DebitTotal),
            Money(line.CreditTotal))));

        return Task.FromResult(Export(
            "Trial Balance",
            companyName,
            generatedAt,
            DateRange(report.FromDate, report.ToDate),
            Row("Account", "Debit", "Credit"),
            rows,
            "trial-balance",
            format));
    }

    public Task<ReportExportFileDto> ExportIncomeStatementAsync(
        IncomeStatementReportDto report,
        ReportExportFormat format,
        string companyName,
        DateTime generatedAt,
        CancellationToken cancellationToken = default)
    {
        var rows = new List<IReadOnlyList<string>>
        {
            Row("Total revenues", Money(report.TotalRevenues)),
            Row("Total expenses", Money(report.TotalExpenses)),
            Row("Net profit", Money(report.NetProfit)),
            EmptyRow(),
            Row("Revenues", "")
        };

        rows.AddRange(report.Revenues.Select(line => Row($"{line.AccountCode} - {line.AccountName}", Money(line.Amount))));
        rows.Add(EmptyRow());
        rows.Add(Row("Expenses", ""));
        rows.AddRange(report.Expenses.Select(line => Row($"{line.AccountCode} - {line.AccountName}", Money(line.Amount))));

        return Task.FromResult(Export(
            "Income Statement",
            companyName,
            generatedAt,
            DateRange(report.FromDate, report.ToDate),
            Row("Item", "Amount"),
            rows,
            "income-statement",
            format));
    }

    public Task<ReportExportFileDto> ExportBalanceSheetAsync(
        BalanceSheetReportDto report,
        ReportExportFormat format,
        string companyName,
        DateTime generatedAt,
        CancellationToken cancellationToken = default)
    {
        var rows = new List<IReadOnlyList<string>>
        {
            Row("Total assets", Money(report.TotalAssets)),
            Row("Total liabilities", Money(report.TotalLiabilities)),
            Row("Total equity", Money(report.TotalEquity)),
            Row("Balanced", report.IsBalanced ? "Yes" : "No"),
            EmptyRow(),
            Row("Assets", "")
        };

        rows.AddRange(report.Assets.Select(line => Row($"{line.AccountCode} - {line.AccountName}", Money(line.Amount))));
        rows.Add(EmptyRow());
        rows.Add(Row("Liabilities", ""));
        rows.AddRange(report.Liabilities.Select(line => Row($"{line.AccountCode} - {line.AccountName}", Money(line.Amount))));
        rows.Add(EmptyRow());
        rows.Add(Row("Equity", ""));
        rows.AddRange(report.Equity.Select(line => Row(line.Name, Money(line.Amount))));

        return Task.FromResult(Export(
            "Balance Sheet",
            companyName,
            generatedAt,
            report.AsOfDate is null ? "As of all dates" : $"As of {report.AsOfDate:yyyy-MM-dd}",
            Row("Item", "Amount"),
            rows,
            "balance-sheet",
            format));
    }

    public Task<ReportExportFileDto> ExportExecutiveSummaryAsync(
        ExecutiveSummaryReportDto report,
        ReportExportFormat format,
        string companyName,
        DateTime generatedAt,
        CancellationToken cancellationToken = default)
    {
        var rows = new List<IReadOnlyList<string>>
        {
            Row("Total revenue", Money(report.TotalRevenue)),
            Row("Total expenses", Money(report.TotalExpenses)),
            Row("Net profit", Money(report.NetProfit)),
            Row("Cash balance", Money(report.CashBalance)),
            Row("Total assets", Money(report.TotalAssets)),
            Row("Total liabilities", Money(report.TotalLiabilities))
        };

        return Task.FromResult(Export(
            "Executive Summary",
            companyName,
            generatedAt,
            DateRange(report.FromDate, report.ToDate),
            Row("Metric", "Amount"),
            rows,
            "executive-summary",
            format));
    }

    public Task<ReportExportFileDto> ExportCashFlowAsync(
        CashFlowSummaryReportDto report,
        ReportExportFormat format,
        string companyName,
        DateTime generatedAt,
        CancellationToken cancellationToken = default)
    {
        var rows = new List<IReadOnlyList<string>>
        {
            Row("Cash in", Money(report.CashIn), ""),
            Row("Cash out", Money(report.CashOut), ""),
            Row("Net cash", Money(report.NetCash), ""),
            EmptyRow(),
            Row("Cash accounts", "", "")
        };

        rows.AddRange(report.Accounts.Select(line => Row(
            $"{line.AccountCode} - {line.AccountName}",
            Money(line.CashIn),
            Money(line.CashOut),
            Money(line.NetCash))));

        return Task.FromResult(Export(
            "Cash Flow Summary",
            companyName,
            generatedAt,
            DateRange(report.FromDate, report.ToDate),
            Row("Item", "Cash In", "Cash Out", "Net Cash"),
            rows,
            "cash-flow",
            format));
    }

    private static ReportExportFileDto Export(
        string title,
        string companyName,
        DateTime generatedAt,
        string period,
        IReadOnlyList<string> headers,
        IReadOnlyList<IReadOnlyList<string>> rows,
        string fileNamePrefix,
        ReportExportFormat format)
    {
        var lines = BuildLines(title, companyName, generatedAt, period, headers, rows);

        return format switch
        {
            ReportExportFormat.Pdf => new ReportExportFileDto(
                $"{fileNamePrefix}.pdf",
                PdfContentType,
                BuildPdf(lines)),

            ReportExportFormat.Excel => new ReportExportFileDto(
                $"{fileNamePrefix}.xlsx",
                ExcelContentType,
                BuildXlsx(title, lines)),

            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported export format.")
        };
    }

    private static IReadOnlyList<string> BuildLines(
        string title,
        string companyName,
        DateTime generatedAt,
        string period,
        IReadOnlyList<string> headers,
        IReadOnlyList<IReadOnlyList<string>> rows)
    {
        var lines = new List<string>
        {
            companyName,
            title,
            period,
            $"Generated at {generatedAt:yyyy-MM-dd HH:mm:ss} UTC",
            string.Empty,
            string.Join(" | ", headers)
        };

        lines.AddRange(rows.Select(row => row.Count == 0 ? string.Empty : string.Join(" | ", row)));

        return lines;
    }

    private static byte[] BuildPdf(IReadOnlyList<string> lines)
    {
        var objects = new List<string>();
        var content = new StringBuilder();
        content.AppendLine("BT");
        content.AppendLine("/F1 11 Tf");
        content.AppendLine("50 780 Td");

        foreach (var line in lines.Take(48))
        {
            content.Append('(')
                .Append(EscapePdf(line))
                .AppendLine(") Tj");
            content.AppendLine("0 -16 Td");
        }

        content.AppendLine("ET");
        var contentText = content.ToString();

        objects.Add("1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj\n");
        objects.Add("2 0 obj << /Type /Pages /Kids [3 0 R] /Count 1 >> endobj\n");
        objects.Add("3 0 obj << /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >> endobj\n");
        objects.Add("4 0 obj << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >> endobj\n");
        objects.Add($"5 0 obj << /Length {Encoding.UTF8.GetByteCount(contentText)} >> stream\n{contentText}endstream\nendobj\n");

        var output = new StringBuilder("%PDF-1.4\n");
        var offsets = new List<int> { 0 };

        foreach (var obj in objects)
        {
            offsets.Add(Encoding.UTF8.GetByteCount(output.ToString()));
            output.Append(obj);
        }

        var xrefOffset = Encoding.UTF8.GetByteCount(output.ToString());
        output.AppendLine("xref");
        output.AppendLine($"0 {objects.Count + 1}");
        output.AppendLine("0000000000 65535 f ");

        foreach (var offset in offsets.Skip(1))
        {
            output.AppendLine($"{offset:0000000000} 00000 n ");
        }

        output.AppendLine("trailer");
        output.AppendLine($"<< /Size {objects.Count + 1} /Root 1 0 R >>");
        output.AppendLine("startxref");
        output.AppendLine(xrefOffset.ToString(CultureInfo.InvariantCulture));
        output.AppendLine("%%EOF");

        return Encoding.UTF8.GetBytes(output.ToString());
    }

    private static byte[] BuildXlsx(string sheetName, IReadOnlyList<string> lines)
    {
        using var stream = new MemoryStream();

        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddEntry(archive, "[Content_Types].xml", """
                <?xml version="1.0" encoding="UTF-8"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Default Extension="xml" ContentType="application/xml"/>
                  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                </Types>
                """);

            AddEntry(archive, "_rels/.rels", """
                <?xml version="1.0" encoding="UTF-8"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                </Relationships>
                """);

            AddEntry(archive, "xl/workbook.xml", $$"""
                <?xml version="1.0" encoding="UTF-8"?>
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <sheets>
                    <sheet name="{{XmlEscape(SafeSheetName(sheetName))}}" sheetId="1" r:id="rId1"/>
                  </sheets>
                </workbook>
                """);

            AddEntry(archive, "xl/_rels/workbook.xml.rels", """
                <?xml version="1.0" encoding="UTF-8"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                </Relationships>
                """);

            AddEntry(archive, "xl/worksheets/sheet1.xml", BuildWorksheet(lines));
        }

        return stream.ToArray();
    }

    private static string BuildWorksheet(IReadOnlyList<string> lines)
    {
        var rows = new StringBuilder();

        for (var index = 0; index < lines.Count; index++)
        {
            rows.Append("<row r=\"")
                .Append(index + 1)
                .Append("\"><c t=\"inlineStr\"><is><t>")
                .Append(XmlEscape(lines[index]))
                .AppendLine("</t></is></c></row>");
        }

        return $$"""
            <?xml version="1.0" encoding="UTF-8"?>
            <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
              <sheetData>
                {{rows}}
              </sheetData>
            </worksheet>
            """;
    }

    private static void AddEntry(ZipArchive archive, string path, string content)
    {
        var entry = archive.CreateEntry(path, CompressionLevel.Fastest);
        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
        writer.Write(content.TrimStart());
    }

    private static string SafeSheetName(string sheetName)
    {
        var invalidChars = new[] { ':', '\\', '/', '?', '*', '[', ']' };
        var safeName = invalidChars.Aggregate(sheetName, (current, invalidChar) => current.Replace(invalidChar, '-'));

        return safeName.Length <= 31 ? safeName : safeName[..31];
    }

    private static string EscapePdf(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("(", "\\(", StringComparison.Ordinal)
            .Replace(")", "\\)", StringComparison.Ordinal);
    }

    private static string XmlEscape(string value)
    {
        return SecurityElement.Escape(value) ?? string.Empty;
    }

    private static string DateRange(DateOnly? fromDate, DateOnly? toDate)
    {
        return (fromDate, toDate) switch
        {
            (null, null) => "All dates",
            (not null, null) => $"From {fromDate:yyyy-MM-dd}",
            (null, not null) => $"To {toDate:yyyy-MM-dd}",
            _ => $"{fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}"
        };
    }

    private static string Money(decimal amount)
    {
        return amount.ToString("N2", CultureInfo.InvariantCulture);
    }

    private static IReadOnlyList<string> Row(params string[] values)
    {
        return values;
    }

    private static IReadOnlyList<string> EmptyRow()
    {
        return Array.Empty<string>();
    }
}
