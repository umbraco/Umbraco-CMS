using System.Reflection;

namespace Umbraco.Core.Models
{
    public interface IDeepCloneable
    {
        object DeepClone();
    }
}