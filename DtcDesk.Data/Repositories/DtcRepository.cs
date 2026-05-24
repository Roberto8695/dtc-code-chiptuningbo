using Dapper;
using DtcDesk.Core.Models;
using DtcDesk.Data.Db;

namespace DtcDesk.Data.Repositories;

/// <summary>
/// Repositorio para operaciones CRUD sobre códigos DTC
/// </summary>
public class DtcRepository
{
    private readonly ConnectionFactory _connectionFactory;

    public DtcRepository(ConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Busca un código DTC por su código (case-insensitive)
    /// </summary>
    public async Task<DtcCode?> GetByCodeAsync(string code)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT Id, Code, Description, Category, Source, Notes, FilterTag, Module,
                   ObdType, CreatedAt, UpdatedAt, IsActive
            FROM DtcCodes
            WHERE Code = @Code COLLATE NOCASE AND IsActive = 1
            LIMIT 1;
        ";

        return await connection.QuerySingleOrDefaultAsync<DtcCode>(sql, new { Code = code.ToUpperInvariant() });
    }

    /// <summary>
    /// Busca un código DTC por su código y tipo OBD (case-insensitive)
    /// </summary>
    public async Task<DtcCode?> GetByCodeAsync(string code, string obdType)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT Id, Code, Description, Category, Source, Notes, FilterTag, Module,
                   ObdType, CreatedAt, UpdatedAt, IsActive
            FROM DtcCodes
            WHERE Code = @Code COLLATE NOCASE
              AND ObdType = @ObdType
              AND IsActive = 1
            LIMIT 1;
        ";

        return await connection.QuerySingleOrDefaultAsync<DtcCode>(sql, new
        {
            Code = code.ToUpperInvariant(),
            ObdType = string.IsNullOrWhiteSpace(obdType) ? "OBD-II" : obdType
        });
    }

    /// <summary>
    /// Busca un código DTC por su ID
    /// </summary>
    public async Task<DtcCode?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT Id, Code, Description, Category, Source, Notes, FilterTag, Module,
                   ObdType, CreatedAt, UpdatedAt, IsActive
            FROM DtcCodes
            WHERE Id = @Id AND IsActive = 1
            LIMIT 1;
        ";

        return await connection.QuerySingleOrDefaultAsync<DtcCode>(sql, new { Id = id });
    }

    /// <summary>
    /// Busca múltiples códigos DTC
    /// </summary>
    public async Task<IEnumerable<DtcCode>> GetByCodesAsync(IEnumerable<string> codes)
    {
        if (!codes.Any())
            return Enumerable.Empty<DtcCode>();

        using var connection = _connectionFactory.CreateConnection();
        
        var normalizedCodes = codes.Select(c => c.ToUpperInvariant()).ToList();

        const string sql = @"
            SELECT Id, Code, Description, Category, Source, Notes, FilterTag, Module,
                   ObdType, CreatedAt, UpdatedAt, IsActive
            FROM DtcCodes
            WHERE Code IN @Codes AND IsActive = 1;
        ";

        return await connection.QueryAsync<DtcCode>(sql, new { Codes = normalizedCodes });
    }

    /// <summary>
    /// Busca múltiples códigos DTC filtrando por tipo OBD
    /// </summary>
    public async Task<IEnumerable<DtcCode>> GetByCodesAsync(IEnumerable<string> codes, string obdType)
    {
        if (!codes.Any())
            return Enumerable.Empty<DtcCode>();

        using var connection = _connectionFactory.CreateConnection();

        var normalizedCodes = codes.Select(c => c.ToUpperInvariant()).ToList();

        const string sql = @"
            SELECT Id, Code, Description, Category, Source, Notes, FilterTag, Module,
                   ObdType, CreatedAt, UpdatedAt, IsActive
            FROM DtcCodes
            WHERE Code IN @Codes
              AND ObdType = @ObdType
              AND IsActive = 1;
        ";

        return await connection.QueryAsync<DtcCode>(sql, new
        {
            Codes = normalizedCodes,
            ObdType = string.IsNullOrWhiteSpace(obdType) ? "OBD-II" : obdType
        });
    }

    /// <summary>
    /// Obtiene todos los códigos DTC activos
    /// </summary>
    public async Task<IEnumerable<DtcCode>> GetAllAsync(bool includeInactive = false)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = includeInactive 
            ? "SELECT * FROM DtcCodes ORDER BY Code;"
            : "SELECT * FROM DtcCodes WHERE IsActive = 1 ORDER BY Code;";

        return await connection.QueryAsync<DtcCode>(sql);
    }

    /// <summary>
    /// Obtiene códigos por categoría
    /// </summary>
    public async Task<IEnumerable<DtcCode>> GetByCategoryAsync(string category)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT * FROM DtcCodes
            WHERE Category = @Category AND IsActive = 1
            ORDER BY Code;
        ";

        return await connection.QueryAsync<DtcCode>(sql, new { Category = category });
    }

    /// <summary>
    /// Busca códigos por texto en descripción
    /// </summary>
    public async Task<IEnumerable<DtcCode>> SearchAsync(string searchTerm)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT * FROM DtcCodes
            WHERE (Code LIKE @SearchPattern OR Description LIKE @SearchPattern)
              AND IsActive = 1
            ORDER BY Code
            LIMIT 100;
        ";

        var searchPattern = $"%{searchTerm}%";
        return await connection.QueryAsync<DtcCode>(sql, new { SearchPattern = searchPattern });
    }

    /// <summary>
    /// Inserta un nuevo código DTC
    /// </summary>
    public async Task<int> InsertAsync(DtcCode dtcCode)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            INSERT INTO DtcCodes (Code, Description, Category, Source, Notes, FilterTag, Module, ObdType, CreatedAt, IsActive)
            VALUES (@Code, @Description, @Category, @Source, @Notes, @FilterTag, @Module, @ObdType, @CreatedAt, @IsActive);
            SELECT last_insert_rowid();
        ";

        dtcCode.Code = dtcCode.Code.ToUpperInvariant();
        dtcCode.CreatedAt = DateTime.UtcNow;

        return await connection.ExecuteScalarAsync<int>(sql, dtcCode);
    }

    /// <summary>
    /// Inserta múltiples códigos en una transacción (optimizado para importaciones masivas)
    /// </summary>
    public async Task<int> BulkInsertAsync(IEnumerable<DtcCode> dtcCodes)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();
        try
        {
            const string sql = @"
                INSERT OR IGNORE INTO DtcCodes (Code, Description, Category, Source, Notes, FilterTag, Module, ObdType, CreatedAt, IsActive)
                VALUES (@Code, @Description, @Category, @Source, @Notes, @FilterTag, @Module, @ObdType, @CreatedAt, @IsActive);
            ";

            var codes = dtcCodes.Select(c =>
            {
                c.Code = c.Code.ToUpperInvariant();
                c.CreatedAt = DateTime.UtcNow;
                return c;
            }).ToList();

            var affectedRows = await connection.ExecuteAsync(sql, codes, transaction);
            
            await transaction.CommitAsync();
            return affectedRows;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Inserta o actualiza múltiples códigos (útil para importaciones JSON).
    /// </summary>
    public async Task<int> UpsertManyAsync(IEnumerable<DtcCode> dtcCodes)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();
        try
        {
            const string sql = @"
                INSERT INTO DtcCodes (Code, Description, Category, Source, Notes, FilterTag, Module, ObdType, CreatedAt, UpdatedAt, IsActive)
                VALUES (@Code, @Description, @Category, @Source, @Notes, @FilterTag, @Module, @ObdType, @CreatedAt, @UpdatedAt, @IsActive)
                ON CONFLICT(Code, ObdType) DO UPDATE SET
                    Description = excluded.Description,
                    Category = excluded.Category,
                    Source = excluded.Source,
                    Notes = excluded.Notes,
                    FilterTag = excluded.FilterTag,
                    Module = excluded.Module,
                    UpdatedAt = excluded.UpdatedAt,
                    IsActive = excluded.IsActive;
            ";

            var now = DateTime.UtcNow;
            var codes = dtcCodes.Select(c =>
            {
                c.Code = c.Code.ToUpperInvariant();
                c.ObdType = string.IsNullOrWhiteSpace(c.ObdType) ? "OBD-II" : c.ObdType;
                c.CreatedAt = c.CreatedAt == default ? now : c.CreatedAt;
                c.UpdatedAt = now;
                return c;
            }).ToList();

            var affectedRows = await connection.ExecuteAsync(sql, codes, transaction);

            await transaction.CommitAsync();
            return affectedRows;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Actualiza un código DTC existente
    /// </summary>
    public async Task<bool> UpdateAsync(DtcCode dtcCode)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            UPDATE DtcCodes
            SET Description = @Description,
                Category = @Category,
                Source = @Source,
                Notes = @Notes,
                FilterTag = @FilterTag,
                Module = @Module,
                ObdType = @ObdType,
                UpdatedAt = @UpdatedAt,
                IsActive = @IsActive
            WHERE Id = @Id;
        ";

        dtcCode.UpdatedAt = DateTime.UtcNow;

        var affectedRows = await connection.ExecuteAsync(sql, dtcCode);
        return affectedRows > 0;
    }

    /// <summary>
    /// Elimina un código DTC (soft delete)
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            UPDATE DtcCodes
            SET IsActive = 0, UpdatedAt = @UpdatedAt
            WHERE Id = @Id;
        ";

        var affectedRows = await connection.ExecuteAsync(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });
        return affectedRows > 0;
    }

    /// <summary>
    /// Elimina permanentemente un código DTC
    /// </summary>
    public async Task<bool> HardDeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = "DELETE FROM DtcCodes WHERE Id = @Id;";

        var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });
        return affectedRows > 0;
    }

    /// <summary>
    /// Verifica si un código existe
    /// </summary>
    public async Task<bool> ExistsAsync(string code)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = "SELECT COUNT(1) FROM DtcCodes WHERE Code = @Code COLLATE NOCASE;";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { Code = code.ToUpperInvariant() });
        return count > 0;
    }

    /// <summary>
    /// Verifica si un código existe para un tipo OBD específico
    /// </summary>
    public async Task<bool> ExistsAsync(string code, string obdType)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT COUNT(1)
            FROM DtcCodes
            WHERE Code = @Code COLLATE NOCASE
              AND ObdType = @ObdType;
        ";

        var count = await connection.ExecuteScalarAsync<int>(sql, new
        {
            Code = code.ToUpperInvariant(),
            ObdType = string.IsNullOrWhiteSpace(obdType) ? "OBD-II" : obdType
        });

        return count > 0;
    }

    /// <summary>
    /// Obtiene el conteo total de códigos
    /// </summary>
    public async Task<int> GetCountAsync(bool includeInactive = false)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = includeInactive
            ? "SELECT COUNT(*) FROM DtcCodes;"
            : "SELECT COUNT(*) FROM DtcCodes WHERE IsActive = 1;";

        return await connection.ExecuteScalarAsync<int>(sql);
    }

    /// <summary>
    /// Elimina todos los códigos DTC de la base de datos
    /// </summary>
    public async Task<int> DeleteAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = "DELETE FROM DtcCodes;";
        
        return await connection.ExecuteAsync(sql);
    }
}
