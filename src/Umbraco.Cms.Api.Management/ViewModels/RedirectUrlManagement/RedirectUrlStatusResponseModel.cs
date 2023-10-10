using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.RedirectUrlManagement;

namespace Umbraco.Cms.Api.Management.ViewModels.RedirectUrlManagement;

public class RedirectUrlStatusResponseModel : IResponseModel
{
    public RedirectStatus Status { get; set; }

    public bool UserIsAdmin { get; set; }
    public string Type => Constants.UdiEntityType.RedirectUrlStatus;
}
