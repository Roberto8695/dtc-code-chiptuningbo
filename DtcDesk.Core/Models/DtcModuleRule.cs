namespace DtcDesk.Core.Models;

/// <summary>
/// Regla de match exacto: un código DTC específico pertenece a un módulo.
/// Ejemplo: C29E → NOX, P0122 → TVA
/// </summary>
public class DtcModuleRule
{
    public int Id { get; set; }

    /// <summary>
    /// Nombre del filtro al que pertenece (ej. "NOX", "TVA")
    /// </summary>
    public string FilterName { get; set; } = string.Empty;

    /// <summary>
    /// Código DTC exacto en mayúsculas (ej. "C29E", "P0122")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de protocolo asociado a la regla (OBD-II u OBD-I).
    /// </summary>
    public string ObdType { get; set; } = "OBD-II";
}
