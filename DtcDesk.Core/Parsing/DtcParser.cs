using System.Text.RegularExpressions;

namespace DtcDesk.Core.Parsing;

/// <summary>
/// Parser para extraer y normalizar códigos DTC desde texto pegado
/// </summary>
public class DtcParser
{
    // Patrones de regex para diferentes formatos de códigos DTC
    private static readonly Regex PCodePattern = new(@"\b[PCBU]\d{4}\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex HexCodePattern = new(@"\b[0-9A-F]{4}\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    
    // Patrón combinado para detectar cualquier código válido
    private static readonly Regex AllCodesPattern = new(
        @"\b(?:[PCBU]\d{4}|[0-9A-F]{4})\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );

    /// <summary>
    /// Extrae códigos DTC del texto pegado y los normaliza
    /// </summary>
    /// <param name="input">Texto con códigos (horizontal o vertical)</param>
    /// <param name="removeDuplicates">Si debe eliminar duplicados manteniendo el orden</param>
    /// <returns>Lista de códigos normalizados en orden secuencial</returns>
    public List<string> Parse(string input, bool removeDuplicates = true)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new List<string>();

        // 1. Extraer todos los códigos válidos manteniendo el orden
        var matches = AllCodesPattern.Matches(input);
        var codes = new List<string>();

        foreach (Match match in matches)
        {
            var code = NormalizeCode(match.Value);
            
            // Validar que sea un código válido
            if (IsValidCode(code))
            {
                codes.Add(code);
            }
        }

        // 2. Eliminar duplicados manteniendo el primer encuentro (orden de izquierda a derecha)
        if (removeDuplicates)
        {
            codes = codes.Distinct().ToList();
        }

        return codes;
    }

    /// <summary>
    /// Normaliza un código a formato estándar (mayúsculas, sin espacios)
    /// Ejemplos: "c073" → "C073", "p0420" → "P0420", " B1234 " → "B1234"
    /// </summary>
    public string NormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return string.Empty;

        return code.Trim().ToUpperInvariant();
    }

    /// <summary>
    /// Valida si un código tiene el formato correcto
    /// </summary>
    public bool IsValidCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        code = code.Trim().ToUpperInvariant();

        // Validar P-codes, C-codes, B-codes, U-codes (letra + 4 dígitos)
        if (Regex.IsMatch(code, @"^[PCBU]\d{4}$"))
            return true;

        // Validar códigos hexadecimales de 4 caracteres
        if (Regex.IsMatch(code, @"^[0-9A-F]{4}$"))
            return true;

        return false;
    }

    /// <summary>
    /// Detecta el tipo/categoría de un código DTC
    /// </summary>
    public string GetCodeCategory(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return "Unknown";

        code = code.Trim().ToUpperInvariant();

        if (code.StartsWith('P'))
            return "Powertrain";
        if (code.StartsWith('C'))
            return "Chassis";
        if (code.StartsWith('B'))
            return "Body";
        if (code.StartsWith('U'))
            return "Network";
        if (Regex.IsMatch(code, @"^[0-9A-F]{4}$"))
            return "Hex";

        return "Unknown";
    }

    /// <summary>
    /// Limpia el texto de entrada eliminando caracteres no deseados
    /// antes de procesar (opcional, el regex ya maneja esto)
    /// </summary>
    public string CleanInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Reemplaza múltiples espacios/tabs/newlines con un solo espacio
        var cleaned = Regex.Replace(input, @"\s+", " ");
        
        // Elimina caracteres especiales que puedan interferir (mantiene alfanuméricos y espacios)
        cleaned = Regex.Replace(cleaned, @"[^\w\s]", " ");

        return cleaned.Trim();
    }

    /// <summary>
    /// Obtiene estadísticas del parsing
    /// </summary>
    public ParsingStats GetStats(string input)
    {
        var codes = Parse(input, removeDuplicates: false);
        var uniqueCodes = codes.Distinct().ToList();

        return new ParsingStats
        {
            TotalCodesFound = codes.Count,
            UniqueCodesFound = uniqueCodes.Count,
            DuplicatesRemoved = codes.Count - uniqueCodes.Count,
            PowertrainCodes = codes.Count(c => c.StartsWith('P')),
            ChassisCodes = codes.Count(c => c.StartsWith('C')),
            BodyCodes = codes.Count(c => c.StartsWith('B')),
            NetworkCodes = codes.Count(c => c.StartsWith('U')),
            HexCodes = codes.Count(c => Regex.IsMatch(c, @"^[0-9A-F]{4}$"))
        };
    }

    /// <summary>
    /// Parse avanzado con información detallada
    /// </summary>
    public List<ParsedCode> ParseDetailed(string input, bool removeDuplicates = true)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new List<ParsedCode>();

        var matches = AllCodesPattern.Matches(input);
        var parsedCodes = new List<ParsedCode>();
        var seenCodes = new HashSet<string>();
        int position = 0;

        foreach (Match match in matches)
        {
            var code = NormalizeCode(match.Value);
            
            if (!IsValidCode(code))
                continue;

            // Si ya vimos este código y debemos eliminar duplicados, saltarlo
            if (removeDuplicates && seenCodes.Contains(code))
                continue;

            parsedCodes.Add(new ParsedCode
            {
                Code = code,
                OriginalText = match.Value,
                Position = position++,
                Category = GetCodeCategory(code),
                SourceIndex = match.Index
            });

            seenCodes.Add(code);
        }

        return parsedCodes;
    }
}

/// <summary>
/// Estadísticas del proceso de parsing
/// </summary>
public class ParsingStats
{
    public int TotalCodesFound { get; set; }
    public int UniqueCodesFound { get; set; }
    public int DuplicatesRemoved { get; set; }
    public int PowertrainCodes { get; set; }
    public int ChassisCodes { get; set; }
    public int BodyCodes { get; set; }
    public int NetworkCodes { get; set; }
    public int HexCodes { get; set; }

    public override string ToString()
    {
        return $"Total: {TotalCodesFound} | Únicos: {UniqueCodesFound} | Duplicados: {DuplicatesRemoved} | P:{PowertrainCodes} C:{ChassisCodes} B:{BodyCodes} U:{NetworkCodes} Hex:{HexCodes}";
    }
}

/// <summary>
/// Representa un código DTC parseado con metadata
/// </summary>
public class ParsedCode
{
    /// <summary>
    /// Código normalizado (mayúsculas)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Texto original antes de normalizar
    /// </summary>
    public string OriginalText { get; set; } = string.Empty;

    /// <summary>
    /// Posición en la lista (orden de izquierda a derecha)
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Categoría del código (Powertrain, Chassis, Body, Network, Hex)
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Índice en el string original donde se encontró
    /// </summary>
    public int SourceIndex { get; set; }
}
