namespace Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;

public class DocumentPropertyValuePermissionPresentationModel : IPermissionPresentationModel
{
    public required ReferenceByIdModel DocumentType { get; set; }

    public required ReferenceByIdModel PropertyType { get; set; }

    public required ISet<string> Verbs { get; set; }

    public IEnumerable<IPermissionPresentationModel> GetAggregatedModels(IEnumerable<IPermissionPresentationModel> models)
    {
        IEnumerable<((Guid DocumentTypeId, Guid PropertyTypeId) Key, ISet<string> Verbs)> groupedModels = models
            .Cast<DocumentPropertyValuePermissionPresentationModel>()
            .GroupBy(x => (x.DocumentType.Id, x.PropertyType.Id))
            .Select(x => (x.Key, (ISet<string>)x.SelectMany(y => y.Verbs).Distinct().ToHashSet()));

        foreach (((Guid DocumentTypeId, Guid PropertyTypeId) key, ISet<string> verbs) in groupedModels)
        {
            yield return new DocumentPropertyValuePermissionPresentationModel
            {
                DocumentType = new ReferenceByIdModel(key.DocumentTypeId),
                PropertyType = new ReferenceByIdModel(key.PropertyTypeId),
                Verbs = verbs
            };
        }
    }
}
