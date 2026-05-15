using DtcDesk.Core.Models;

namespace DtcDesk.Core.Services;

/// <summary>
/// Servicio de clasificación híbrida de códigos DTC por módulo.
/// Estrategia:
///   1. Match exacto por código (HashSet, O(1))  → para NOX, TVA
///   2. Búsqueda de keywords en descripción      → para VNT, DPF, EGR, SCR, MAF
/// Es case-insensitive en ambas estrategias.
/// </summary>
public class DtcClassifierService
{
    // Diccionario: código normalizado (mayús) → nombre de filtro
    private readonly Dictionary<string, string> _exactRules;

    // Diccionario: nombre de filtro → lista de keywords en minúsculas
    private readonly Dictionary<string, List<string>> _keywordRules;

    /// <summary>
    /// Inicializa el clasificador con las reglas cargadas desde BD.
    /// </summary>
    /// <param name="exactRules">Reglas de match exacto código → filtro</param>
    /// <param name="keywordRules">Reglas de keyword en descripción → filtro</param>
    public DtcClassifierService(
        IEnumerable<DtcModuleRule> exactRules,
        IEnumerable<DtcModuleKeyword> keywordRules)
    {
        // Construir lookup exacto: código UPPER → FilterName
        _exactRules = exactRules
            .ToDictionary(
                r => r.Code.ToUpperInvariant(),
                r => r.FilterName,
                StringComparer.OrdinalIgnoreCase);

        // Agrupar keywords por filtro
        _keywordRules = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var kw in keywordRules)
        {
            if (!_keywordRules.TryGetValue(kw.FilterName, out var list))
            {
                list = new List<string>();
                _keywordRules[kw.FilterName] = list;
            }
            list.Add(kw.Keyword.ToLowerInvariant());
        }
    }

    /// <summary>
    /// Clasifica un código DTC dado su código y descripción.
    /// Devuelve el nombre del filtro detectado (ej. "VNT", "EGR") o null si no hay match.
    /// </summary>
    /// <param name="code">Código DTC normalizado (ej. "P0401", "C29E")</param>
    /// <param name="description">Descripción del código (puede ser null)</param>
    public string? Classify(string code, string? description)
    {
        // 1. Match exacto por código
        var upperCode = code.ToUpperInvariant();
        if (_exactRules.TryGetValue(upperCode, out var exactMatch))
            return exactMatch;

        // Para códigos hex puros (ej. "2122"), intentar variantes con prefijo
        if (upperCode.Length == 4 && upperCode.All(c => Uri.IsHexDigit(c)))
        {
            var prefixedP = "P" + upperCode;
            if (_exactRules.TryGetValue(prefixedP, out exactMatch))
                return exactMatch;

            var prefixedU = "U" + upperCode;
            if (_exactRules.TryGetValue(prefixedU, out exactMatch))
                return exactMatch;
        }

        // 2. Keywords en descripción (solo si hay descripción)
        if (string.IsNullOrWhiteSpace(description))
            return null;

        var lowerDesc = description.ToLowerInvariant();

        foreach (var (filterName, keywords) in _keywordRules)
        {
            foreach (var kw in keywords)
            {
                if (lowerDesc.Contains(kw))
                    return filterName;
            }
        }

        return null;
    }

    /// <summary>
    /// Clasifica un resultado ya construido y asigna su FilterTag.
    /// Devuelve el mismo objeto mutado por conveniencia.
    /// </summary>
    public DtcLookupResult ClassifyResult(DtcLookupResult result)
    {
        result.FilterTag = Classify(result.Code, result.Description);
        return result;
    }

    /// <summary>
    /// Clasifica una lista de resultados en bloque.
    /// </summary>
    public void ClassifyAll(IEnumerable<DtcLookupResult> results)
    {
        foreach (var r in results)
            r.FilterTag = Classify(r.Code, r.Description);
    }

    /// <summary>
    /// Devuelve true si el clasificador tiene reglas cargadas.
    /// Útil para verificar que el seeding ya ocurrió.
    /// </summary>
    public bool HasRules =>
        _exactRules.Count > 0 || _keywordRules.Count > 0;
}
