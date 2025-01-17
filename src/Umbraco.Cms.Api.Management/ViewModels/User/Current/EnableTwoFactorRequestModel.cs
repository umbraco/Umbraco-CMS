namespace Umbraco.Cms.Api.Management.ViewModels.User.Current;

public class EnableTwoFactorRequestModel
{
    public required string Code { get; set; }

    public required string Secret { get; set; }

}
