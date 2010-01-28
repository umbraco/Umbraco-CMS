<%@ Control Language="c#" AutoEventWireup="True" Codebehind="theend.ascx.cs" Inherits="umbraco.presentation.install.steps.theend" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>

<script type="text/javascript">
function openDialog(diaTitle, diaDoc, dwidth, dheight, optionalParams)
{
  theDialogWindow = window.open(diaDoc,'dialogpage', "width="+dwidth+"px,height="+dheight+"px"+optionalParams);// window.showModalDialog(diaDoc, "MyDialog", strFeatures);
}

function runStarterKits() {
  openDialog('packager', 'http://packages.umbraco.org/packages/website-packages?callback=<%=Request.ServerVariables["SERVER_NAME"] + umbraco.IO.SystemDirectories.Umbraco + "/dialogs/packager.aspx" %>', 530, 550, ',scrollbars=yes');
}
</script>


<h1 style="padding: 10px 0px 10px 0px;">Umbraco <%=umbraco.GlobalSettings.CurrentVersion%> is installed and ready for use</h1>

<asp:Panel ID="updateUmbracoSettingsFailed" Visible="false" runat="server">
<div class="error">
<p id="errorDetails"><asp:Literal ID="errorLiteral" runat="server"></asp:Literal></p>
<p> To finish the installation, you'll need to 
manually edit the <strong>/web.config file</strong> and update the AppSetting key <strong>umbracoConfigurationStatus</strong> in the bottom to the value of <strong>'<%=umbraco.GlobalSettings.CurrentVersion %>'</strong>.</p>
</div>
</asp:Panel>

<div class="success">
<p>
You can get <strong>started instantly</strong> by clicking the "Launch Umbraco" button below. <br />If you are <strong>new to umbraco</strong>, 
you can find plenty of resources on our getting started pages.
</p>
</div>


<div class="actions">

<a href="#" id="newsletterSignUp"><h3>Register for security and upgrade notifications</h3>
Receive security bulletins and our monthly newsletter with special offers. No spam, period.
</a>
<div id="newsLetterResponse" style="display: none;">Thank you</div>
<div id="newsLetterForm">
<label for="name">Name</label><input class="text" type="text" name="name" id="name" /><br />
<label for="email">Email</label><input class="text" type="text" name="email" id="email" /><br/>
<p>
<input type="button" id="subscribeButton" value="Register" />
</p>
</div>

<div style="display: none">
<asp:PlaceHolder ID="viewSite" runat="server" Visible="false">
<a href="/" target="_blank"><h3>Browse your new site</h3>
You installed Runway, so why not see how your new website looks.
</a>
</asp:PlaceHolder>
</div>

<a href="<%= umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco) %>/umbraco.aspx"><h3>Launch Umbraco</h3>
To manage your website, simply open the umbraco back office and start adding content, updating the templates and stylesheets or add new functionality
</a>

<a href="http://umbraco.org/documentation/getting-started" target="_blank"><h3>Further help and information</h3>
Get help from our award winning community, browse the documentation or watch some free videos on how to build a simple site, how to use packages and a quick guide to the umbraco terminology
</a>
</div>

<script type="text/javascript">
  jQuery(document).ready(function() {
    jQuery("#contentScroll").css("height", "600px");
    jQuery("#name").val("").focus();
    
    
    jQuery("#newsletterSignUp").click(function() {
      jQuery("#newsLetterResponse").hide();
      jQuery("#newsLetterForm").slideDown("normal");
      jQuery("#name").val("").focus();
      jQuery("#email").val("");
      return false;
    });

    jQuery("#subscribeButton").click(function() {

    jQuery("#name").removeClass("errorField");
    jQuery("#email").removeClass("errorField");
    
    if (jQuery("#name").val() == "") { jQuery("#name").addClass("errorField"); return false; }
    if (jQuery("#email").val() == "") { jQuery("#email").addClass("errorField"); return false; }

      jQuery.post("default.aspx", $("form").serialize());

      jQuery("#newsLetterForm").slideUp("normal");
      jQuery("#newsLetterResponse").show();

      jQuery("#name").val("").removeClass("errorField");
      jQuery("#email").val("").removeClass("errorField");
      
    });

  });
</script>

