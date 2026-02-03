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
    /// Ruta por defecto de la base de datos (en carpeta Data del programa)
    /// </summary>
    public static string GetDefaultDatabasePath()
    {
        // Obtener el directorio base de la aplicación
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var dataFolder = Path.Combine(baseDirectory, "Data");
        
        if (!Directory.Exists(dataFolder))
        {
            Directory.CreateDirectory(dataFolder);
        }

        return Path.Combine(dataFolder, "dtc_codes.db");
    }
}
