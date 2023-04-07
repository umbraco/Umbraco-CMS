﻿using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Web.BackOffice.Security;

public class BackOfficePasswordChanger : IBackOfficePasswordChanger
{
    private readonly IPasswordChanger<BackOfficeIdentityUser> _passwordChanger;
    private readonly IBackOfficeUserManager _userManager;

    public BackOfficePasswordChanger(
        IPasswordChanger<BackOfficeIdentityUser> passwordChanger,
        IBackOfficeUserManager userManager)
    {
        _passwordChanger = passwordChanger;
        _userManager = userManager;
    }

    public async Task<Attempt<PasswordChangedModel?>> ChangeBackOfficePassword(
        ChangeBackOfficeUserPasswordModel model)
    {
        var mappedModel = new ChangingPasswordModel
        {
            Id = model.User.Id,
            OldPassword = model.OldPassword,
            NewPassword = model.NewPassword
        };

        return await _passwordChanger.ChangePasswordWithIdentityAsync(mappedModel, _userManager);
    }
}
