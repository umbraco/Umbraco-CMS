using System;
using System.Configuration.Provider;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Umbraco.Web.Models;

namespace umbraco.controls
{
    public partial class passwordChanger : System.Web.UI.UserControl
    {
        public string MembershipProviderName { get; set; }

        protected MembershipProvider Provider
        {
            get { return Membership.Providers[MembershipProviderName]; }
        }

        public ChangingPasswordModel ChangingPasswordModel
        {
            get
            {
                var model = new ChangingPasswordModel
                    {
                        NewPassword = umbPasswordChanger_passwordNew.Text,
                        OldPassword = umbPasswordChanger_passwordCurrent.Text,
                        Reset = ResetPasswordCheckBox.Checked
                    };
                return model;
            }
        }

        public string Password
        {
            get { return ChangingPasswordModel.NewPassword; }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (Membership.Providers[MembershipProviderName] == null)
            {
                throw new ProviderException("The membership provider " + MembershipProviderName + " was not found");
            }

            //TODO: WE need to support this! - requires UI updates, etc...
            if (Provider.RequiresQuestionAndAnswer)
            {
                throw new NotSupportedException("Currently the user editor does not support providers that have RequiresQuestionAndAnswer specified");
            }
        }

        /// <summary>
        /// umbPasswordChanger_passwordNew control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox umbPasswordChanger_passwordNew;

        /// <summary>
        /// umbPasswordChanger_passwordNewConfirm control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox umbPasswordChanger_passwordNewConfirm;

        /// <summary>
        /// CompareValidator1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.CompareValidator ConfirmPasswordValidator;

        /// <summary>
        /// IsChangingPassword control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.HiddenField IsChangingPassword;

        /// <summary>
        /// ResetPasswordCheckBox control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.CheckBox ResetPasswordCheckBox;

        /// <summary>
        /// umbPasswordChanger_passwordCurrent control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox umbPasswordChanger_passwordCurrent;
    }
}