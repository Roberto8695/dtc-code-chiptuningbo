using Microsoft.Data.Sqlite;

namespace DtcDesk.Data.Db;

/// <summary>
/// Factory para crear conexiones a la base de datos SQLite
/// </summary>
public class ConnectionFactory
{
    private readonly string _connectionString;

    public ConnectionFactory(string databasePath)
    {
        // Construye el connection string para SQLite
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared
        };

        _connectionString = builder.ToString();

        // Asegura que el directorio exista
        var directory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    /// <summary>
    /// Crea una nueva conexión a la base de datos
    /// </summary>
    public SqliteConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }

    /// <summary>
    /// Obtiene el connection string
    /// </summary>
    public string GetConnectionString()
    {
        return _connectionString;
    }

    /// <summary>
    /// Ruta por defecto de la base de datos (en AppData)
    /// </summary>
    public static string GetDefaultDatabasePath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "DtcDesk");
        
        if (!Directory.Exists(appFolder))
        {
            Directory.CreateDirectory(appFolder);
        }

        return Path.Combine(appFolder, "dtc_codes.db");
    }
}
