namespace DtcDesk.Core.Models;

public class DtcExportPackage
{
    public string Version { get; set; } = "1.0";
    public DateTime ExportedAtUtc { get; set; } = DateTime.UtcNow;
    public List<DtcCode> Codes { get; set; } = new();
    public List<DtcModuleFilter>? Modules { get; set; }
    public List<DtcModuleRule>? ExactRules { get; set; }
    public List<DtcModuleKeyword>? Keywords { get; set; }
}
