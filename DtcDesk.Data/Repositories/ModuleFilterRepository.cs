using DtcDesk.Core.Models;
using Microsoft.Data.Sqlite;

namespace DtcDesk.Data.Repositories;

/// <summary>
/// Repositorio para gestionar las reglas de clasificación de módulos DTC.
/// Usa Microsoft.Data.Sqlite directamente (sin Dapper) para mantener coherencia
/// con la decisión de no usar EF.
/// </summary>
public class ModuleFilterRepository
{
    private readonly string _connectionString;

    public ModuleFilterRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    // ─────────────────────────────────────────────────────────────
    // CONSULTAS
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Obtiene todos los módulos/filtros definidos, ordenados por SortOrder.
    /// </summary>
    public async Task<List<DtcModuleFilter>> GetAllFiltersAsync()
    {
        var filters = new List<DtcModuleFilter>();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, DisplayName, Description, SortOrder FROM DtcModuleFilters ORDER BY SortOrder;";

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            filters.Add(new DtcModuleFilter
            {
                Id          = reader.GetInt32(0),
                Name        = reader.GetString(1),
                DisplayName = reader.GetString(2),
                Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                SortOrder   = reader.GetInt32(4)
            });
        }

        return filters;
    }

    /// <summary>
    /// Obtiene todas las reglas de match exacto (código → filtro).
    /// </summary>
    public async Task<List<DtcModuleRule>> GetAllExactRulesAsync()
    {
        var rules = new List<DtcModuleRule>();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, FilterName, Code FROM DtcModuleExactRules;";

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            rules.Add(new DtcModuleRule
            {
                Id         = reader.GetInt32(0),
                FilterName = reader.GetString(1),
                Code       = reader.GetString(2)
            });
        }

        return rules;
    }

    /// <summary>
    /// Obtiene todas las keywords de clasificación (descripción → filtro).
    /// </summary>
    public async Task<List<DtcModuleKeyword>> GetAllKeywordsAsync()
    {
        var keywords = new List<DtcModuleKeyword>();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, FilterName, Keyword FROM DtcModuleKeywords;";

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            keywords.Add(new DtcModuleKeyword
            {
                Id         = reader.GetInt32(0),
                FilterName = reader.GetString(1),
                Keyword    = reader.GetString(2)
            });
        }

        return keywords;
    }

    /// <summary>
    /// Verifica si ya existen reglas en la base de datos.
    /// Usado para no ejecutar el seeder más de una vez.
    /// </summary>
    public async Task<bool> HasAnyRulesAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(1) FROM DtcModuleFilters;";

        var count = await cmd.ExecuteScalarAsync();
        return Convert.ToInt64(count) > 0;
    }

    /// <summary>
    /// Guarda (inserta o actualiza) la regla exacta código → módulo.
    /// Si ya existía una regla para ese código, la actualiza al nuevo módulo.
    /// </summary>
    public async Task SaveExactRuleAsync(string code, string filterName)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        await using var cmd = connection.CreateCommand();
        // Primero intentamos actualizar; si no existe, insertamos
        cmd.CommandText = @"
            INSERT INTO DtcModuleExactRules (FilterName, Code)
            VALUES (@FilterName, @Code)
            ON CONFLICT(FilterName, Code) DO NOTHING;

            -- Si el mismo código ya tenía un filtro diferente, actualizarlo
            UPDATE DtcModuleExactRules
            SET FilterName = @FilterName
            WHERE Code = @Code COLLATE NOCASE AND FilterName != @FilterName;
        ";
        cmd.Parameters.AddWithValue("@FilterName", filterName);
        cmd.Parameters.AddWithValue("@Code", code.ToUpperInvariant());
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Elimina cualquier regla exacta asociada a un código (usado cuando se selecciona "Ninguno").
    /// </summary>
    public async Task DeleteExactRuleByCodeAsync(string code)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM DtcModuleExactRules WHERE Code = @Code COLLATE NOCASE;";
        cmd.Parameters.AddWithValue("@Code", code.ToUpperInvariant());
        await cmd.ExecuteNonQueryAsync();
    }

    // ─────────────────────────────────────────────────────────────
    // SEEDING INICIAL
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Inserta las reglas iniciales del cliente si la base aún está vacía.
    /// Seguro para llamar en cada startup (idempotente con la comprobación previa).
    /// </summary>
    public async Task SeedDefaultRulesAsync()
    {
        if (await HasAnyRulesAsync())
            return;   // Ya sembrado, no hacer nada

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            // 1. Insertar módulos
            await using (var cmd = connection.CreateCommand())
            {
                cmd.Transaction = (SqliteTransaction)transaction;
                cmd.CommandText = @"
                    INSERT OR IGNORE INTO DtcModuleFilters (Name, DisplayName, Description, SortOrder)
                    VALUES (@Name, @DisplayName, @Desc, @Sort);";

                foreach (var (name, display, desc, sort) in DtcDesk.Data.Db.ModuleRulesSeeder.DefaultFilters)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@Name",    name);
                    cmd.Parameters.AddWithValue("@DisplayName", display);
                    cmd.Parameters.AddWithValue("@Desc",    desc);
                    cmd.Parameters.AddWithValue("@Sort",    sort);
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            // 2. Insertar reglas exactas
            await using (var cmd = connection.CreateCommand())
            {
                cmd.Transaction = (SqliteTransaction)transaction;
                cmd.CommandText = @"
                    INSERT OR IGNORE INTO DtcModuleExactRules (FilterName, Code)
                    VALUES (@FilterName, @Code);";

                foreach (var (filterName, code) in DtcDesk.Data.Db.ModuleRulesSeeder.DefaultExactRules)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@FilterName", filterName);
                    cmd.Parameters.AddWithValue("@Code",       code.ToUpperInvariant());
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            // 3. Insertar keywords
            await using (var cmd = connection.CreateCommand())
            {
                cmd.Transaction = (SqliteTransaction)transaction;
                cmd.CommandText = @"
                    INSERT OR IGNORE INTO DtcModuleKeywords (FilterName, Keyword)
                    VALUES (@FilterName, @Keyword);";

                foreach (var (filterName, keyword) in DtcDesk.Data.Db.ModuleRulesSeeder.DefaultKeywordRules)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@FilterName", filterName);
                    cmd.Parameters.AddWithValue("@Keyword",    keyword.ToLowerInvariant());
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
