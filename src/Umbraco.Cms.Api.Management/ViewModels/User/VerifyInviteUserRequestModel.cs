using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class VerifyInviteUserRequestModel
{
    [Required]
    public Guid UserId { get; set; } = Guid.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;
}
