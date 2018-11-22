namespace Umbraco.Core.Models.EntityBase
{
    /// <summary>
    /// An interface that defines if the object is tracking property changes and that is is also
    /// remembering what property changes had been made after the changes were committed.
    /// </summary>
    public interface IRememberBeingDirty : ICanBeDirty
    {
        bool WasDirty();
        bool WasPropertyDirty(string propertyName);
        void ForgetPreviouslyDirtyProperties();
        void ResetDirtyProperties(bool rememberPreviouslyChangedProperties);
    }
}