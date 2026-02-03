using Microsoft.Data.Sqlite;

namespace DtcDesk.Data.Db;

/// <summary>
/// Inicializa la base de datos SQLite creando tablas e índices
/// </summary>
public class DbInitializer
{
    private readonly string _connectionString;

    public DbInitializer(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Crea la base de datos y las tablas si no existen
    /// </summary>
    public void Initialize()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var createTableSql = @"
            CREATE TABLE IF NOT EXISTS DtcCodes (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Code TEXT NOT NULL UNIQUE COLLATE NOCASE,
                Description TEXT NOT NULL,
                Category TEXT,
                Source TEXT,
                Notes TEXT,
                CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
                UpdatedAt TEXT,
                IsActive INTEGER NOT NULL DEFAULT 1
            );

            -- Índice en Code para búsquedas ultra-rápidas (crítico con 2000+ registros)
            CREATE INDEX IF NOT EXISTS idx_dtc_code 
                ON DtcCodes(Code COLLATE NOCASE);

            -- Índice en Category para filtros por tipo
            CREATE INDEX IF NOT EXISTS idx_dtc_category 
                ON DtcCodes(Category);

            -- Índice en IsActive para consultas de códigos activos
            CREATE INDEX IF NOT EXISTS idx_dtc_active 
                ON DtcCodes(IsActive);

            -- Índice compuesto para búsquedas filtradas
            CREATE INDEX IF NOT EXISTS idx_dtc_code_active 
                ON DtcCodes(Code, IsActive);
        ";

        using var command = connection.CreateCommand();
        command.CommandText = createTableSql;
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Verifica la integridad de la base de datos
    /// </summary>
    public bool CheckIntegrity()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA integrity_check;";
        
        var result = command.ExecuteScalar()?.ToString();
        return result == "ok";
    }

    /// <summary>
    /// Optimiza la base de datos (vacuum y reindex)
    /// Útil después de importaciones masivas
    /// </summary>
    public void Optimize()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "VACUUM; REINDEX;";
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Obtiene estadísticas de la base de datos
    /// </summary>
    public DatabaseStats GetStats()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT 
                COUNT(*) as TotalCodes,
                COUNT(CASE WHEN IsActive = 1 THEN 1 END) as ActiveCodes,
                COUNT(CASE WHEN Category = 'P' THEN 1 END) as PowertrainCodes,
                COUNT(CASE WHEN Category = 'C' THEN 1 END) as ChassisCodes,
                COUNT(CASE WHEN Category = 'B' THEN 1 END) as BodyCodes,
                COUNT(CASE WHEN Category = 'U' THEN 1 END) as NetworkCodes
            FROM DtcCodes;
        ";

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new DatabaseStats
            {
                TotalCodes = reader.GetInt32(0),
                ActiveCodes = reader.GetInt32(1),
                PowertrainCodes = reader.GetInt32(2),
                ChassisCodes = reader.GetInt32(3),
                BodyCodes = reader.GetInt32(4),
                NetworkCodes = reader.GetInt32(5)
            };
        }

        return new DatabaseStats();
    }
}

/// <summary>
/// Estadísticas de la base de datos
/// </summary>
public class DatabaseStats
{
    public int TotalCodes { get; set; }
    public int ActiveCodes { get; set; }
    public int PowertrainCodes { get; set; }
    public int ChassisCodes { get; set; }
    public int BodyCodes { get; set; }
    public int NetworkCodes { get; set; }

    public override string ToString()
    {
        return $"Total: {TotalCodes} | Activos: {ActiveCodes} | P:{PowertrainCodes} C:{ChassisCodes} B:{BodyCodes} U:{NetworkCodes}";
    }
}
