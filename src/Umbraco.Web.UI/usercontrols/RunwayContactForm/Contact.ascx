<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Contact.ascx.cs" Inherits="UmbracoShop.Controls.Contact" %>

<asp:ValidationSummary id="valSum" runat="server" CssClass="error" DisplayMode="BulletList" />

<fieldset>
<legend>Your details</legend>
<p><asp:Label id="lb_name" runat="server" AssociatedControlID="tb_name" Text="Name" /><asp:TextBox ID="tb_name" cssclass="text" runat="server" /></p>
<p><asp:Label id="lb_email" runat="server" AssociatedControlID="tb_email" Text="Email" /><asp:TextBox ID="tb_email" cssclass="text" runat="server" /></p>
<p><asp:Label id="lb_company" runat="server" AssociatedControlID="tb_company" Text="Company" /><asp:TextBox ID="tb_company" cssclass="text" runat="server" /></p>
</fieldset>
<fieldset>
<legend>Message</legend>
<p><asp:Label id="Label1" runat="server" AssociatedControlID="tb_msg" Text="Message" /> <asp:textbox ID="tb_msg" runat="server" cssclass="text" textmode="MultiLine"/></p>
<p><asp:Button ID="bt_submit" OnClick="sendMail" runat="server" Text="Send email" /> <asp:Label CssClass="success" id="lb_success" runat="server" Visible="false">Email send</asp:Label></p>
</fieldset>

<asp:RequiredFieldValidator Display="None" ID="RequiredFieldValidator0" ControlToValidate="tb_name" runat="server" ErrorMessage="Name is mandatory" />
<asp:RequiredFieldValidator Display="None" ID="RequiredFieldValidator1" ControlToValidate="tb_email" runat="server" ErrorMessage="Email is mandatory" />
<asp:RequiredFieldValidator Display="None" ID="RequiredFieldValidator2" ControlToValidate="tb_msg" runat="server" ErrorMessage="Message is mandatory" />

<asp:RegularExpressionValidator Display="None" ID="RegularExpressionValidator0" runat="server" ValidationExpression="^(?i:(?<local_part>[a-z0-9!#$%^&*{}'`+=-_|/?]+(?:\.[a-z0-9!#$%^&*{}'`+=-_|/?]+)*)@(?<labels>[a-z0-9]+\z?.*[a-z0-9-_]+)*(?<tld>\.[a-z0-9]{2,}))$" ControlToValidate="tb_email" ErrorMessage="Email is not valid" />
