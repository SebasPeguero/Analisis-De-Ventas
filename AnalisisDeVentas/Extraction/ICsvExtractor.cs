using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnalisisDeVentas.Extraction;





public interface ICsvExtractor<T>
{
    
    
    
    
    
    Task<IEnumerable<T>> ExtractAsync(string filePath);
}