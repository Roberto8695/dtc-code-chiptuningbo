namespace DtcDesk.Core.Models;

/// <summary>
/// Resultado de una operación de importación masiva de códigos
/// </summary>
public class ImportResult
{
    /// <summary>
    /// Cantidad total de registros procesados
    /// </summary>
    public int TotalProcessed { get; set; }

    /// <summary>
    /// Cantidad de registros insertados exitosamente
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Cantidad de registros actualizados
    /// </summary>
    public int UpdatedCount { get; set; }

    /// <summary>
    /// Cantidad de registros con errores
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Cantidad de registros duplicados (omitidos)
    /// </summary>
    public int DuplicateCount { get; set; }

    /// <summary>
    /// Lista de errores ocurridos durante la importación
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Indica si la importación fue completamente exitosa
    /// </summary>
    public bool IsSuccess => ErrorCount == 0 && TotalProcessed > 0;

    /// <summary>
    /// Mensaje resumen de la importación
    /// </summary>
    public string GetSummary()
    {
        return $"Procesados: {TotalProcessed} | Insertados: {SuccessCount} | Actualizados: {UpdatedCount} | Errores: {ErrorCount} | Duplicados: {DuplicateCount}";
    }
}
