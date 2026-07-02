using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

namespace AdoNet_EfCore.Extraction;

/// <summary>
/// Implementación genérica para la extracción de datos desde archivos CSV utilizando CsvHelper.
/// </summary>
/// <typeparam name="T">El tipo de DTO a retornar.</typeparam>
public class CsvExtractor<T> : ICsvExtractor<T>
{
    /// <summary>
    /// Extrae los registros del archivo CSV especificado de forma asíncrona.
    /// </summary>
    /// <param name="filePath">Ruta física del archivo CSV.</param>
    /// <returns>Colección de objetos mapeados.</returns>
    public async Task<IEnumerable<T>> ExtractAsync(string filePath)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);

        var records = new List<T>();
        await foreach (var record in csv.GetRecordsAsync<T>())
        {
            records.Add(record);
        }

        return records;
    }
}
