using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;

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
                Code TEXT NOT NULL COLLATE NOCASE,
                Description TEXT NOT NULL,
                Category TEXT,
                Source TEXT,
                Notes TEXT,
                FilterTag TEXT,
                Module TEXT,
                ObdType TEXT NOT NULL DEFAULT 'OBD-II',
                CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
                UpdatedAt TEXT,
                IsActive INTEGER NOT NULL DEFAULT 1,
                UNIQUE(Code, ObdType)
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

            -- ─────────────────────────────────────────────────────
            -- TABLAS DE CLASIFICACIÓN POR MÓDULO (VNT, DPF, EGR…)
            -- ─────────────────────────────────────────────────────

            -- Módulos/filtros disponibles
            CREATE TABLE IF NOT EXISTS DtcModuleFilters (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                Name        TEXT NOT NULL UNIQUE COLLATE NOCASE,
                DisplayName TEXT NOT NULL,
                Description TEXT,
                SortOrder   INTEGER NOT NULL DEFAULT 0,
                IsSystem    INTEGER NOT NULL DEFAULT 0
            );

            -- Reglas de match exacto: un código DTC concreto → módulo
            CREATE TABLE IF NOT EXISTS DtcModuleExactRules (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                FilterName  TEXT NOT NULL COLLATE NOCASE,
                Code        TEXT NOT NULL COLLATE NOCASE,
                UNIQUE(FilterName, Code)
            );

            CREATE INDEX IF NOT EXISTS idx_exact_rules_code
                ON DtcModuleExactRules(Code COLLATE NOCASE);

            -- Palabras clave en descripción → módulo
            CREATE TABLE IF NOT EXISTS DtcModuleKeywords (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                FilterName  TEXT NOT NULL COLLATE NOCASE,
                Keyword     TEXT NOT NULL COLLATE NOCASE,
                UNIQUE(FilterName, Keyword)
            );

            CREATE INDEX IF NOT EXISTS idx_keywords_filter
                ON DtcModuleKeywords(FilterName COLLATE NOCASE);
        ";

        using var command = connection.CreateCommand();
        command.CommandText = createTableSql;
        command.ExecuteNonQuery();

        EnsureColumnExists(connection, "DtcCodes", "FilterTag", "TEXT");
        EnsureColumnExists(connection, "DtcCodes", "Module", "TEXT");
        EnsureColumnExists(connection, "DtcCodes", "ObdType", "TEXT NOT NULL DEFAULT 'OBD-II'");
        EnsureColumnExists(connection, "DtcModuleFilters", "IsSystem", "INTEGER NOT NULL DEFAULT 0");

        EnsureObdTypeCompositeKey(connection);

        using var moduleIndexCommand = connection.CreateCommand();
        moduleIndexCommand.CommandText = @"
            CREATE INDEX IF NOT EXISTS idx_dtc_module
                ON DtcCodes(Module);
        ";
        moduleIndexCommand.ExecuteNonQuery();

        using var markSystemModulesCommand = connection.CreateCommand();
        markSystemModulesCommand.CommandText = @"
            UPDATE DtcModuleFilters
            SET IsSystem = 1
            WHERE UPPER(Name) IN ('VNT', 'DPF', 'EGR', 'NOX', 'SCR', 'MAF', 'TVA');
        ";
        markSystemModulesCommand.ExecuteNonQuery();
    }

    private static void EnsureObdTypeCompositeKey(SqliteConnection connection)
    {
        var hasComposite = false;
        var hasCodeUnique = false;

        using (var listCmd = connection.CreateCommand())
        {
            listCmd.CommandText = "PRAGMA index_list('DtcCodes');";
            using var listReader = listCmd.ExecuteReader();
            while (listReader.Read())
            {
                var indexName = listReader.GetString(1);
                var isUnique = listReader.GetBoolean(2);
                if (!isUnique)
                {
                    continue;
                }

                using var infoCmd = connection.CreateCommand();
                infoCmd.CommandText = $"PRAGMA index_info('{indexName}');";
                using var infoReader = infoCmd.ExecuteReader();
                var columns = new List<string>();
                while (infoReader.Read())
                {
                    columns.Add(infoReader.GetString(2));
                }

                if (columns.Count == 2
                    && columns.Any(c => string.Equals(c, "Code", StringComparison.OrdinalIgnoreCase))
                    && columns.Any(c => string.Equals(c, "ObdType", StringComparison.OrdinalIgnoreCase)))
                {
                    hasComposite = true;
                }
                else if (columns.Count == 1
                    && columns.Any(c => string.Equals(c, "Code", StringComparison.OrdinalIgnoreCase)))
                {
                    hasCodeUnique = true;
                }
            }
        }

        if (hasComposite || !hasCodeUnique)
        {
            return;
        }

        using var transaction = connection.BeginTransaction();
        try
        {
            using var createCmd = connection.CreateCommand();
            createCmd.Transaction = transaction;
            createCmd.CommandText = @"
                CREATE TABLE DtcCodes_New (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Code TEXT NOT NULL COLLATE NOCASE,
                    Description TEXT NOT NULL,
                    Category TEXT,
                    Source TEXT,
                    Notes TEXT,
                    FilterTag TEXT,
                    Module TEXT,
                    ObdType TEXT NOT NULL DEFAULT 'OBD-II',
                    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL DEFAULT 1,
                    UNIQUE(Code, ObdType)
                );
            ";
            createCmd.ExecuteNonQuery();

            using var copyCmd = connection.CreateCommand();
            copyCmd.Transaction = transaction;
            copyCmd.CommandText = @"
                INSERT INTO DtcCodes_New
                (Id, Code, Description, Category, Source, Notes, FilterTag, Module, ObdType, CreatedAt, UpdatedAt, IsActive)
                SELECT
                    Id, Code, Description, Category, Source, Notes, FilterTag, Module,
                    COALESCE(ObdType, 'OBD-II'), CreatedAt, UpdatedAt, IsActive
                FROM DtcCodes;
            ";
            copyCmd.ExecuteNonQuery();

            using var dropCmd = connection.CreateCommand();
            dropCmd.Transaction = transaction;
            dropCmd.CommandText = "DROP TABLE DtcCodes;";
            dropCmd.ExecuteNonQuery();

            using var renameCmd = connection.CreateCommand();
            renameCmd.Transaction = transaction;
            renameCmd.CommandText = "ALTER TABLE DtcCodes_New RENAME TO DtcCodes;";
            renameCmd.ExecuteNonQuery();

            using var indexCmd = connection.CreateCommand();
            indexCmd.Transaction = transaction;
            indexCmd.CommandText = @"
                CREATE INDEX IF NOT EXISTS idx_dtc_code
                    ON DtcCodes(Code COLLATE NOCASE);
                CREATE INDEX IF NOT EXISTS idx_dtc_category
                    ON DtcCodes(Category);
                CREATE INDEX IF NOT EXISTS idx_dtc_active
                    ON DtcCodes(IsActive);
                CREATE INDEX IF NOT EXISTS idx_dtc_code_active
                    ON DtcCodes(Code, IsActive);
                CREATE INDEX IF NOT EXISTS idx_dtc_module
                    ON DtcCodes(Module);
            ";
            indexCmd.ExecuteNonQuery();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static void EnsureColumnExists(SqliteConnection connection, string tableName, string columnName, string columnType)
    {
        using var existsCommand = connection.CreateCommand();
        existsCommand.CommandText = $"PRAGMA table_info({tableName});";

        using var reader = existsCommand.ExecuteReader();
        var exists = false;
        while (reader.Read())
        {
            var existingColumn = reader.GetString(1);
            if (string.Equals(existingColumn, columnName, StringComparison.OrdinalIgnoreCase))
            {
                exists = true;
                break;
            }
        }

        if (!exists)
        {
            using var alterCommand = connection.CreateCommand();
            alterCommand.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnType};";
            alterCommand.ExecuteNonQuery();
        }
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
