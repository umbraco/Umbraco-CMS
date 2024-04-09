using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Security;

public interface IUserForgotPasswordSender
{
    Task SendForgotPassword(UserForgotPasswordMessage message);

    bool CanSend();
}
