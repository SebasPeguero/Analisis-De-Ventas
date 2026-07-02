using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdoNet_EfCore.Extraction;

/// <summary>
/// Define el contrato para la extracción de datos desde archivos CSV.
/// </summary>
/// <typeparam name="T">El tipo de DTO a retornar.</typeparam>
public interface ICsvExtractor<T>
{
    /// <summary>
    /// Extrae los registros de un archivo CSV y los retorna en una colección tipada de forma asíncrona.
    /// </summary>
    /// <param name="filePath">Ruta física del archivo CSV.</param>
    /// <returns>Colección de registros extraídos.</returns>
    Task<IEnumerable<T>> ExtractAsync(string filePath);
}
