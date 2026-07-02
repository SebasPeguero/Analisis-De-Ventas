using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;

namespace AnalisisDeVentas.Data;




public class AdoConnectionFactory
{
    private readonly string _connectionString;

    
    
    
    
    public AdoConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SistemaAnalisisVentas")
            ?? throw new InvalidOperationException("La cadena de conexión 'SistemaAnalisisVentas' no está configurada en appsettings.json.");
    }

    
    
    
    
    public SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}