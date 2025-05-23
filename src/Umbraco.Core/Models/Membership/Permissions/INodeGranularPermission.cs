namespace Umbraco.Cms.Core.Models.Membership.Permissions;

public interface INodeGranularPermission : IGranularPermission
{
    new Guid Key { get; set; }

    Guid? IGranularPermission.Key
    {
        get => Key;
    }
}
