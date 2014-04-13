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
using Umbraco.Core;
using Umbraco.Core.Security;
using Umbraco.Web.Models;

namespace umbraco.controls
{
    public partial class passwordChanger : UserControl
    {
        public string MembershipProviderName { get; set; }

        protected MembershipProvider Provider
        {
            get { return Membership.Providers[MembershipProviderName]; }
        }

        private bool? _showOldPassword;

        /// <summary>
        /// Determines whether to show the old password field or not
        /// </summary>
        internal protected bool ShowOldPassword
        {
            get
            {
                if (_showOldPassword.HasValue == false)
                {
                    var umbProvider = Provider as MembershipProviderBase;
                    if (umbProvider != null && umbProvider.AllowManuallyChangingPassword)
                    {
                        _showOldPassword = false;
                    }
                    else
                    {
                        _showOldPassword = Provider.EnablePasswordRetrieval == false;
                    }
                }
                return _showOldPassword.Value;
            }
            internal set { _showOldPassword = value; }
        }

        public bool IsChangingPassword
        {
            get
            {
                var convertAttempt = IsChangingPasswordField.Value.TryConvertTo<bool>();
                return convertAttempt.Success && convertAttempt.Result;
            }
        }

        private readonly ChangingPasswordModel _model = new ChangingPasswordModel();

        public ChangingPasswordModel ChangingPasswordModel
        {
            get
            {
                _model.NewPassword = umbPasswordChanger_passwordNew.Text;
                _model.OldPassword = umbPasswordChanger_passwordCurrent.Text;
                _model.Reset = ResetPasswordCheckBox.Checked;           
                return _model;
            }
        }

        [Obsolete("Use the ChangingPasswordModel instead")]
        public string Password
        {
            get { return ChangingPasswordModel.NewPassword; }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

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
        protected global::System.Web.UI.WebControls.HiddenField IsChangingPasswordField;

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