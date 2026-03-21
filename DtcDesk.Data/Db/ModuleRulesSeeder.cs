namespace DtcDesk.Data.Db;

/// <summary>
/// Define las reglas iniciales de clasificación por módulo según las especificaciones del cliente.
/// Se usa una sola vez al inicializar la base de datos si las tablas están vacías.
/// </summary>
public static class ModuleRulesSeeder
{
    /// <summary>
    /// Módulos con su nombre para UI y descripción del sistema
    /// </summary>
    public static readonly (string Name, string DisplayName, string Description, int SortOrder)[] DefaultFilters =
    {
        ("VNT",  "VNT",  "Variable Nozzle Turbine — Sistema turbocompresor",                  1),
        ("DPF",  "DPF",  "Diesel Particulate Filter — Filtro de partículas",                   2),
        ("EGR",  "EGR",  "Exhaust Gas Recirculation — Recirculación de gases de escape",       3),
        ("NOX",  "NOX",  "Sensor/catalizador de óxidos de nitrógeno",                          4),
        ("SCR",  "SCR",  "Selective Catalytic Reduction — Sistema AdBlue/Urea",                5),
        ("MAF",  "MAF",  "Mass/Manifold Air Flow — Sensor de flujo/presión de aire",           6),
        ("TVA",  "TVA",  "Throttle Valve Actuator — Actuador de mariposa",                     7),
    };

    /// <summary>
    /// Reglas de match exacto: código DTC → FilterName.
    /// Prioridad máxima sobre keywords.
    /// </summary>
    public static readonly (string FilterName, string Code)[] DefaultExactRules =
    {
        // NOX — solo estos dos códigos específicos
        ("NOX", "C29E"),
        ("NOX", "C29D"),

        // TVA — lista exacta del cliente
        ("TVA", "P0122"),
        ("TVA", "P0123"),
        ("TVA", "P2100"),
        ("TVA", "P2101"),
        ("TVA", "P2103"),
    };

    /// <summary>
    /// Reglas por keyword en descripción: keyword (minúsculas) → FilterName.
    /// Se aplican si no hay match exacto.
    /// </summary>
    public static readonly (string FilterName, string Keyword)[] DefaultKeywordRules =
    {
        // VNT — todo relacionado con turbo
        ("VNT", "vnt"),
        ("VNT", "turbo"),
        ("VNT", "boost"),
        ("VNT", "wastegate"),
        ("VNT", "charge air"),
        ("VNT", "turbocharger"),
        ("VNT", "variable geometry"),
        ("VNT", "variable nozzle"),

        // DPF — filtro de partículas y regeneración
        ("DPF", "dpf"),
        ("DPF", "particulate"),
        ("DPF", "soot"),
        ("DPF", "catalyst"),
        ("DPF", "catalytic"),
        ("DPF", "filter"),
        ("DPF", "regeneration"),
        ("DPF", "differential pressure"),

        // EGR — recirculación de gases
        ("EGR", "egr"),
        ("EGR", "exhaust gas recirculation"),
        ("EGR", "recirculation"),

        // SCR — sistema AdBlue/urea
        ("SCR", "scr"),
        ("SCR", "adblue"),
        ("SCR", "urea"),
        ("SCR", "reductant"),
        ("SCR", "selective catalytic"),
        ("SCR", "def "),           // Diesel Exhaust Fluid (con espacio para evitar falsos positivos)

        // MAF — sensores de aire
        ("MAF", "maf"),
        ("MAF", " map "),          // Manifold Absolute Pressure (con espacios)
        ("MAF", "mass air flow"),
        ("MAF", "mass airflow"),
        ("MAF", "iat"),
        ("MAF", "intake air temperature"),
        ("MAF", "barometric pressure"),
        ("MAF", "air flow sensor"),
        ("MAF", "air meter"),
    };
}
