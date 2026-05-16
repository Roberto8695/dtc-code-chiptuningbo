using Microsoft.Data.Sqlite;

namespace DtcDesk.Data.Db;

/// <summary>
/// Factory para crear conexiones a la base de datos SQLite
/// </summary>
public class ConnectionFactory
{
    private const string AppName = "DTCDesk";
    private const string LegacyDataFolderName = "Data";
    private const string UserDataFolderName = "UserData";
    private const string DatabaseFileName = "dtc_codes.db";

    private readonly string _connectionString;

    public ConnectionFactory(string databasePath)
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared
        };

        _connectionString = builder.ToString();

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
    /// Ruta por defecto de la base de datos en una carpeta de usuario persistente.
    /// Si detecta una instalación antigua con la BD dentro de la carpeta del programa,
    /// la migra automáticamente para no perder información durante las actualizaciones.
    /// </summary>
    public static string GetDefaultDatabasePath()
    {
        var installDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var userDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AppName,
            UserDataFolderName);

        if (!Directory.Exists(userDataFolder))
        {
            Directory.CreateDirectory(userDataFolder);
        }

        var targetDbPath = Path.Combine(userDataFolder, DatabaseFileName);
        TryMigrateLegacyDatabase(installDirectory, targetDbPath);

        return targetDbPath;
    }

    private static void TryMigrateLegacyDatabase(string installDirectory, string targetDbPath)
    {
        if (File.Exists(targetDbPath))
        {
            return;
        }

        var legacyDataFolder = Path.Combine(installDirectory, LegacyDataFolderName);
        var legacyDbPath = Path.Combine(legacyDataFolder, DatabaseFileName);
        if (!File.Exists(legacyDbPath))
        {
            return;
        }

        try
        {
            var targetDirectory = Path.GetDirectoryName(targetDbPath);
            if (!string.IsNullOrWhiteSpace(targetDirectory) && !Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            File.Copy(legacyDbPath, targetDbPath, overwrite: false);
            CopyIfExists(legacyDbPath + "-wal", targetDbPath + "-wal");
            CopyIfExists(legacyDbPath + "-shm", targetDbPath + "-shm");
        }
        catch
        {
            // Si la migración falla por permisos o bloqueo, la app continuará usando
            // la nueva ruta y SQLite creará una BD nueva. Evitamos romper el arranque.
            // El instalador no elimina la BD antigua, así que se puede recuperar luego.
        }
    }

    private static void CopyIfExists(string sourcePath, string destinationPath)
    {
        if (!File.Exists(sourcePath) || File.Exists(destinationPath))
        {
            return;
        }

        File.Copy(sourcePath, destinationPath, overwrite: false);
    }
}
