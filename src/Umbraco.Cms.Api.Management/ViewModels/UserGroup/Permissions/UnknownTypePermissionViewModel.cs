namespace Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;

public class UnknownTypePermissionPresentationModel : IPermissionPresentationModel
{
    public required ISet<string> Verbs { get; set; }

    public required string Context { get; set; }

    public IEnumerable<IPermissionPresentationModel> GetAggregatedModels(IEnumerable<IPermissionPresentationModel> models)
    {
        IEnumerable<(string Context, ISet<string> Verbs)> groupedModels = models
            .Cast<UnknownTypePermissionPresentationModel>()
            .GroupBy(x => x.Context)
            .Select(x => (x.Key, (ISet<string>)x.SelectMany(y => y.Verbs).Distinct().ToHashSet()));

        foreach ((string context, ISet<string> verbs) in groupedModels)
        {
            yield return new UnknownTypePermissionPresentationModel
            {
                Context = context,
                Verbs = verbs
            };
        }
    }
}
