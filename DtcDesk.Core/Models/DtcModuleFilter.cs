namespace DtcDesk.Core.Models;

/// <summary>
/// Representa un módulo/filtro de clasificación DTC (VNT, DPF, EGR, etc.)
/// </summary>
public class DtcModuleFilter
{
    public int Id { get; set; }

    /// <summary>
    /// Nombre interno del filtro (ej. "VNT", "DPF"). Usado como clave.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Nombre descriptivo para la UI
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del sistema al que aplica
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Orden de visualización en la UI
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Indica si el módulo es de sistema (predefinido) o personalizado por el usuario.
    /// </summary>
    public bool IsSystem { get; set; }
}
