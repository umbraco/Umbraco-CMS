using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Models;

public class UserForgotPasswordMessage
{
    public required Uri ForgotPasswordUri { get; set; }

    public required IUser Recipient { get; set; }
}
