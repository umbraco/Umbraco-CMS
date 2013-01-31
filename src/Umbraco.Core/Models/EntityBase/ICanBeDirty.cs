namespace Umbraco.Core.Models.EntityBase
{
    /// <summary>
    /// An interface that defines the object is tracking property changes and if it is Dirty
    /// </summary>
    public interface ICanBeDirty
    {
        bool IsDirty();
        bool IsPropertyDirty(string propName);
        void ResetDirtyProperties();
    }
}