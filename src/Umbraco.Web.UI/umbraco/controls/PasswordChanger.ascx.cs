using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Linq;
using System.Web;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Security;

namespace Umbraco.Web.UI.Umbraco.Controls
{
    public partial class PasswordChanger : global::umbraco.controls.passwordChanger
    {
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            //always reset the control vals
            ResetPasswordCheckBox.Checked = false;
            umbPasswordChanger_passwordCurrent.Text = null;
            umbPasswordChanger_passwordNew.Text = null;
            umbPasswordChanger_passwordNewConfirm.Text = null;
            //reset the flag always
            IsChangingPasswordField.Value = "false";

            var canReset = Provider.CanResetPassword(ApplicationContext.Current.Services.UserService);

            ResetPlaceHolder.Visible = canReset;

            this.DataBind();
        }

    }
}