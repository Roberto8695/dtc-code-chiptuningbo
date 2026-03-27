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
        cmd.CommandText = "SELECT Id, Name, DisplayName, Description, SortOrder, IsSystem FROM DtcModuleFilters ORDER BY SortOrder, DisplayName;";

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            filters.Add(new DtcModuleFilter
            {
                Id          = reader.GetInt32(0),
                Name        = reader.GetString(1),
                DisplayName = reader.GetString(2),
                Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                SortOrder   = reader.GetInt32(4),
                IsSystem    = !reader.IsDBNull(5) && reader.GetInt64(5) == 1
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
    /// Obtiene todos los códigos exactos asociados a un filtro.
    /// </summary>
    public async Task<List<string>> GetExactCodesByFilterAsync(string filterName)
    {
        var codes = new List<string>();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT Code
            FROM DtcModuleExactRules
            WHERE FilterName = @FilterName COLLATE NOCASE
            ORDER BY Code;";
        cmd.Parameters.AddWithValue("@FilterName", filterName);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            codes.Add(reader.GetString(0));
        }

        return codes;
    }

    /// <summary>
    /// Inserta un módulo personalizado y reemplaza sus códigos exactos asociados.
    /// </summary>
    public async Task<int> CreateCustomFilterAsync(string displayName, string? description, IEnumerable<string> exactCodes)
    {
        var normalizedDisplayName = displayName.Trim();
        var internalName = BuildUniqueInternalName(normalizedDisplayName);

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            var sortOrder = await GetNextSortOrderAsync(connection, (SqliteTransaction)transaction);

            await using var insertFilterCmd = connection.CreateCommand();
            insertFilterCmd.Transaction = (SqliteTransaction)transaction;
            insertFilterCmd.CommandText = @"
                INSERT INTO DtcModuleFilters (Name, DisplayName, Description, SortOrder, IsSystem)
                VALUES (@Name, @DisplayName, @Description, @SortOrder, 0);
                SELECT last_insert_rowid();";
            insertFilterCmd.Parameters.AddWithValue("@Name", internalName);
            insertFilterCmd.Parameters.AddWithValue("@DisplayName", normalizedDisplayName);
            insertFilterCmd.Parameters.AddWithValue("@Description", (object?)description ?? DBNull.Value);
            insertFilterCmd.Parameters.AddWithValue("@SortOrder", sortOrder);

            var createdId = Convert.ToInt32(await insertFilterCmd.ExecuteScalarAsync());

            await ReplaceExactRulesAsync(connection, (SqliteTransaction)transaction, internalName, exactCodes);

            await transaction.CommitAsync();
            return createdId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Actualiza un módulo personalizado y reemplaza sus códigos exactos asociados.
    /// </summary>
    public async Task UpdateCustomFilterAsync(int filterId, string displayName, string? description, IEnumerable<string> exactCodes)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            var filter = await GetFilterByIdAsync(connection, (SqliteTransaction)transaction, filterId);
            if (filter == null)
            {
                throw new InvalidOperationException("El módulo no existe.");
            }

            if (filter.IsSystem)
            {
                throw new InvalidOperationException("No se puede editar un módulo de sistema.");
            }

            await using (var updateFilterCmd = connection.CreateCommand())
            {
                updateFilterCmd.Transaction = (SqliteTransaction)transaction;
                updateFilterCmd.CommandText = @"
                    UPDATE DtcModuleFilters
                    SET DisplayName = @DisplayName,
                        Description = @Description
                    WHERE Id = @Id;";
                updateFilterCmd.Parameters.AddWithValue("@DisplayName", displayName.Trim());
                updateFilterCmd.Parameters.AddWithValue("@Description", (object?)description ?? DBNull.Value);
                updateFilterCmd.Parameters.AddWithValue("@Id", filterId);
                await updateFilterCmd.ExecuteNonQueryAsync();
            }

            await ReplaceExactRulesAsync(connection, (SqliteTransaction)transaction, filter.Name, exactCodes);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Elimina un módulo personalizado y sus reglas asociadas.
    /// </summary>
    public async Task DeleteCustomFilterAsync(int filterId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            var filter = await GetFilterByIdAsync(connection, (SqliteTransaction)transaction, filterId);
            if (filter == null)
            {
                return;
            }

            if (filter.IsSystem)
            {
                throw new InvalidOperationException("No se puede eliminar un módulo de sistema.");
            }

            await DeleteRulesByFilterAsync(connection, (SqliteTransaction)transaction, filter.Name);

            await using var deleteFilterCmd = connection.CreateCommand();
            deleteFilterCmd.Transaction = (SqliteTransaction)transaction;
            deleteFilterCmd.CommandText = "DELETE FROM DtcModuleFilters WHERE Id = @Id;";
            deleteFilterCmd.Parameters.AddWithValue("@Id", filterId);
            await deleteFilterCmd.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
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
                    INSERT OR IGNORE INTO DtcModuleFilters (Name, DisplayName, Description, SortOrder, IsSystem)
                    VALUES (@Name, @DisplayName, @Desc, @Sort, 1);";

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

    private static string BuildUniqueInternalName(string displayName)
    {
        var cleaned = new string(displayName
            .ToUpperInvariant()
            .Where(char.IsLetterOrDigit)
            .ToArray());

        if (string.IsNullOrWhiteSpace(cleaned))
        {
            cleaned = "MOD";
        }

        var suffix = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
        return $"USR_{cleaned}_{suffix}";
    }

    private static async Task<int> GetNextSortOrderAsync(SqliteConnection connection, SqliteTransaction transaction)
    {
        await using var cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = "SELECT COALESCE(MAX(SortOrder), 0) + 1 FROM DtcModuleFilters;";
        var next = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(next);
    }

    private static async Task<DtcModuleFilter?> GetFilterByIdAsync(SqliteConnection connection, SqliteTransaction transaction, int filterId)
    {
        await using var cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = @"
            SELECT Id, Name, DisplayName, Description, SortOrder, IsSystem
            FROM DtcModuleFilters
            WHERE Id = @Id;";
        cmd.Parameters.AddWithValue("@Id", filterId);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new DtcModuleFilter
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            DisplayName = reader.GetString(2),
            Description = reader.IsDBNull(3) ? null : reader.GetString(3),
            SortOrder = reader.GetInt32(4),
            IsSystem = !reader.IsDBNull(5) && reader.GetInt64(5) == 1
        };
    }

    private static async Task ReplaceExactRulesAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string filterName,
        IEnumerable<string> exactCodes)
    {
        await DeleteRulesByFilterAsync(connection, transaction, filterName);

        var normalizedCodes = exactCodes
            .Select(c => c.Trim().ToUpperInvariant())
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalizedCodes.Count == 0)
        {
            return;
        }

        await using var insertRuleCmd = connection.CreateCommand();
        insertRuleCmd.Transaction = transaction;
        insertRuleCmd.CommandText = @"
            INSERT INTO DtcModuleExactRules (FilterName, Code)
            VALUES (@FilterName, @Code)
            ON CONFLICT(FilterName, Code) DO NOTHING;

            UPDATE DtcModuleExactRules
            SET FilterName = @FilterName
            WHERE Code = @Code COLLATE NOCASE AND FilterName != @FilterName;";

        foreach (var code in normalizedCodes)
        {
            insertRuleCmd.Parameters.Clear();
            insertRuleCmd.Parameters.AddWithValue("@FilterName", filterName);
            insertRuleCmd.Parameters.AddWithValue("@Code", code);
            await insertRuleCmd.ExecuteNonQueryAsync();
        }
    }

    private static async Task DeleteRulesByFilterAsync(SqliteConnection connection, SqliteTransaction transaction, string filterName)
    {
        await using var deleteExactCmd = connection.CreateCommand();
        deleteExactCmd.Transaction = transaction;
        deleteExactCmd.CommandText = "DELETE FROM DtcModuleExactRules WHERE FilterName = @FilterName COLLATE NOCASE;";
        deleteExactCmd.Parameters.AddWithValue("@FilterName", filterName);
        await deleteExactCmd.ExecuteNonQueryAsync();

        await using var deleteKeywordCmd = connection.CreateCommand();
        deleteKeywordCmd.Transaction = transaction;
        deleteKeywordCmd.CommandText = "DELETE FROM DtcModuleKeywords WHERE FilterName = @FilterName COLLATE NOCASE;";
        deleteKeywordCmd.Parameters.AddWithValue("@FilterName", filterName);
        await deleteKeywordCmd.ExecuteNonQueryAsync();
    }
}
