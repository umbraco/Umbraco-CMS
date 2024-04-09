namespace Umbraco.Cms.Api.Management.ViewModels.Template.Query;

public class TemplateQueryExecuteModel
{
    public ReferenceByIdModel? RootDocument { get; set; }

    public string? DocumentTypeAlias { get; set; }

    public IEnumerable<TemplateQueryExecuteFilterPresentationModel>? Filters { get; set; }

    public TemplateQueryExecuteSortModel? Sort { get; set; }

    public int Take { get; set; }

}
