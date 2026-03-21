namespace DtcDesk.Core.Models;

/// <summary>
/// Palabra clave que, al aparecer en la descripción de un DTC, indica que pertenece a un módulo.
/// Ejemplo: "turbo" → VNT, "egr" → EGR
/// </summary>
public class DtcModuleKeyword
{
    public int Id { get; set; }

    /// <summary>
    /// Nombre del filtro al que aplica esta keyword (ej. "VNT", "EGR")
    /// </summary>
    public string FilterName { get; set; } = string.Empty;

    /// <summary>
    /// Palabra clave en minúsculas (la búsqueda es case-insensitive)
    /// </summary>
    public string Keyword { get; set; } = string.Empty;
}
