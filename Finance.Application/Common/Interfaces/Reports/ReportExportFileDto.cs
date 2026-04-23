namespace Finance.Application.Common.Interfaces.Reports;

public record ReportExportFileDto(
    string FileName,
    string ContentType,
    byte[] Content);
