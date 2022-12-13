using Umbraco.New.Cms.Core.Models.RedirectUrlManagement;

namespace Umbraco.Cms.Api.Management.ViewModels.RedirectUrlManagement;

public class RedirectUrlStatusViewModel
{
    public RedirectStatus Status { get; set; }

    public bool UserIsAdmin { get; set; }
}
