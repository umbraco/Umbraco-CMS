namespace Umbraco.Cms.Api.Management.ViewModels;

public interface INamedEntityPresentationModel
{
    Guid Key { get; }

    string Name { get;}
}
