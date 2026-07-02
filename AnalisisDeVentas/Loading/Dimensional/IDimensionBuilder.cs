using System.Threading.Tasks;

namespace AnalisisDeVentas.Loading.Dimensional;

public interface IDimensionBuilder
{
    Task BuildDimensionsAsync();
    Task BuildFactsAsync();
}
