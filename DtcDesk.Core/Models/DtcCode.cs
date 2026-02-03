namespace DtcDesk.Core.Models;

/// <summary>
/// Representa un código DTC (Diagnostic Trouble Code) con su descripción
/// </summary>
public class DtcCode
{
    /// <summary>
    /// ID único del código en la base de datos
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Código DTC (ej. P0420, C073, B1234, U0100)
    /// Siempre en mayúsculas, formato normalizado
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Descripción completa del error/falla
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Categoría del código DTC:
    /// - P = Powertrain (motor/transmisión)
    /// - C = Chassis (frenos, suspensión)
    /// - B = Body (carrocería, electrónica)
    /// - U = Network (comunicación CAN)
    /// - Hex = Códigos hexadecimales genéricos
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Fuente del código (ej. "OBD-II Standard", "VAG Group", "BMW", etc.)
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Notas adicionales o información técnica
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Fecha de creación del registro
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Última fecha de actualización
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Indica si el código está activo/habilitado para búsquedas
    /// </summary>
    public bool IsActive { get; set; } = true;
}
