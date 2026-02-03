namespace DtcDesk.Core.Models;

/// <summary>
/// Resultado de la búsqueda de un código DTC
/// </summary>
public class DtcLookupResult
{
    /// <summary>
    /// Código DTC buscado (normalizado)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Indica si el código fue encontrado en el diccionario
    /// </summary>
    public bool Found { get; set; }

    /// <summary>
    /// Descripción del código (null si no se encontró)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Categoría del código (null si no se encontró)
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Fuente del código (null si no se encontró)
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Notas adicionales (null si no se encontró)
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Estado asignado por el usuario (0, F, etc.)
    /// Usado para clasificación y exportación
    /// </summary>
    public string? UserStatus { get; set; }

    /// <summary>
    /// ID del código en la base de datos (0 si no se encontró)
    /// </summary>
    public int? DtcId { get; set; }
}
