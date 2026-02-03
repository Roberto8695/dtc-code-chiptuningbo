using DtcDesk.Core.Parsing;

namespace DtcDesk.Core.Services;

/// <summary>
/// Servicio principal para procesar códigos DTC
/// Combina el parser con el repositorio para lookup completo
/// </summary>
public class DtcService
{
    private readonly DtcParser _parser;

    public DtcService()
    {
        _parser = new DtcParser();
    }

    /// <summary>
    /// Procesa texto pegado y extrae códigos en orden
    /// </summary>
    public List<string> ProcessPastedText(string input)
    {
        return _parser.Parse(input, removeDuplicates: true);
    }

    /// <summary>
    /// Procesa y obtiene información detallada de cada código
    /// </summary>
    public List<ParsedCode> ProcessPastedTextDetailed(string input)
    {
        return _parser.ParseDetailed(input, removeDuplicates: true);
    }

    /// <summary>
    /// Valida si un código es válido
    /// </summary>
    public bool ValidateCode(string code)
    {
        return _parser.IsValidCode(code);
    }

    /// <summary>
    /// Obtiene estadísticas del texto pegado
    /// </summary>
    public ParsingStats GetParsingStatistics(string input)
    {
        return _parser.GetStats(input);
    }
}
