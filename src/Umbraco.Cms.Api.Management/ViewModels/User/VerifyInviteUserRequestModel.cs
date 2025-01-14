using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class VerifyInviteUserRequestModel
{
    public required ReferenceByIdModel User { get; set; }

    [Required]
    public string Token { get; set; } = string.Empty;
}
