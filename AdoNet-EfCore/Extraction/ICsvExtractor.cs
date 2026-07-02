using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdoNet_EfCore.Extraction;





public interface ICsvExtractor<T>
{
    
    
    
    
    
    Task<IEnumerable<T>> ExtractAsync(string filePath);
}