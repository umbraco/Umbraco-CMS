namespace Umbraco.Cms.Core.Models.Membership.Permissions;

public interface IExternalGranularPermission : IGranularPermission
{
    new Guid Key { get; set; }

    Guid? IGranularPermission.Key
    {
        get => Key;
    }
}
