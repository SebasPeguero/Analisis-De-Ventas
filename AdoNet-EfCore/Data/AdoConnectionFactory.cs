using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;

namespace AdoNet_EfCore.Data;

/// <summary>
/// Fábrica de conexiones ADO.NET para encapsular la construcción de objetos SqlConnection.
/// </summary>
public class AdoConnectionFactory
{
    private readonly string _connectionString;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="AdoConnectionFactory"/> leyendo la cadena de conexión de la configuración.
    /// </summary>
    /// <param name="configuration">Instancia del sistema de configuración de .NET.</param>
    public AdoConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SistemaAnalisisVentas")
            ?? throw new InvalidOperationException("La cadena de conexión 'SistemaAnalisisVentas' no está configurada en appsettings.json.");
    }

    /// <summary>
    /// Crea y retorna una nueva instancia de <see cref="SqlConnection"/> configurada.
    /// </summary>
    /// <returns>Objeto de conexión SQL Server.</returns>
    public SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
