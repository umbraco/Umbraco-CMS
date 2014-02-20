namespace Umbraco.Core.Models.EntityBase
{
    public interface IDeepCloneable
    {
        T DeepClone<T>() where T : IDeepCloneable;
    }
}