<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="insertLink.aspx.cs" Inherits="umbraco.presentation.plugins.tinymce3.insertLink" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<%@ Register TagPrefix="ui" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register Src="../../controls/Tree/TreeControl.ascx" TagName="TreeControl" TagPrefix="umbraco" %>
<%@ Register TagPrefix="umbClient" Namespace="Umbraco.Web.UI.Bundles" Assembly="umbraco" %>


<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">

<head id="Head1" runat="server">
    <title>{#advlink_dlg.title}</title>
    
     <base target="_self" />

    <ui:UmbracoClientDependencyLoader runat="server" id="ClientLoader" />
	
	
    <umbClient:JsApplicationLib runat="server" />
    <umbClient:JsJQueryCore runat="server" />
    <umbClient:JsUmbracoApplicationCore runat="server" />

	<umb:JsInclude ID="JsInclude1" runat="server" FilePath="tinymce3/tiny_mce_popup.js" PathNameAlias="UmbracoClient" Priority="100" />
	<umb:JsInclude ID="JsInclude3" runat="server" FilePath="tinymce3/plugins/umbracolink/js/umbracolink.js" PathNameAlias="UmbracoClient" Priority="101" />
	<umb:JsInclude ID="JsInclude4" runat="server" FilePath="tinymce3/utils/form_utils.js" PathNameAlias="UmbracoClient" Priority="102" />
    <umb:JsInclude ID="JsInclude5" runat="server" FilePath="tinymce3/utils/validate.js" PathNameAlias="UmbracoClient" Priority="103" />
   

    <script type="text/javascript">
        var currentLink = "";

        function dialogHandler(id) {
            id = id.toString();
            if (id == "-1") return;
            var returnValues = id.split("|");
            if (returnValues.length > 1) {
                if (returnValues[1] != '')
                    setFormValue('href', returnValues[1]);
                else
                    setFormValue('href', returnValues[0]);
                
                setFormValue('localUrl', returnValues[0]);
                setFormValue('title', returnValues[2]);
            } else {
                if (id.substring(id.length - 1, id.length) == "|")
                  id = id.substring(0, id.length - 1);

                setFormValue('href', id);
                setFormValue('localUrl', id);

                //umbraco.presentation.webservices.legacyAjaxCalls.NiceUrl(id, updateInternalLink, updateInternalLinkError);
            }
        }

        function validateUmbracoLink(link) {
            if (link.indexOf('{localLink') > -1) {
                // check for / prefix
                if (link.substring(0, 1) == "/") {
                    link = link.substring(1, link.length);
                }

                // update internal link ref
                setFormValue('localUrl', link);
                currentLink = link;
                
                // show friendly url
                umbraco.presentation.webservices.legacyAjaxCalls.NiceUrl(link.substring(11, link.length - 1), updateInternalLink, updateInternalLinkError);
                
                return "Updating internal link...";
            } else {
                return link;
            }
        }

        function updateInternalLink(result) {
            if (result != "")
                setFormValue('href', result);
            else
                setFormValue('href', currentLink);
        }

        function updateInternalLinkError(error) {
            // don't show the error, but just revert to the old link...
            setFormValue('href', currentLink);
        }
    </script>
    
    
</head>
<body id="advlink" style="display: none">
	
	

    <form runat="server" action="#">
    <asp:ScriptManager EnablePartialRendering="false" runat="server">
        <Services>
            <asp:ServiceReference Path="../../webservices/legacyAjaxCalls.asmx" />
        </Services>
    </asp:ScriptManager>
    
    <ui:Pane runat="server" ID="pane_url">
      <ui:PropertyPanel runat="server" Text="Url">
           <input type="hidden" id="localUrl" name="localUrl" onchange="" />
           <input id="href" name="href" type="text" style="width: 220px;" value="" onchange="document.getElementById('localUrl').value = ''; selectByValue(this.form,'linklisthref',this.value);" />
      </ui:PropertyPanel>
      
      <ui:PropertyPanel ID="PropertyPanel1" runat="server" Text="Title">
          <input id="title" name="title" type="text" value="" style="width: 220px;" />
      </ui:PropertyPanel>
      
      <ui:PropertyPanel ID="PropertyPanel2" runat="server" Text="Target">
          <div id="targetlistcontainer"></div>
      </ui:PropertyPanel>
        
      <div id="anchorlistrow">
      <ui:PropertyPanel ID="PropertyPanel3" runat="server" Text="Anchor">
          <div id="anchorlistcontainer"></div>
      </ui:PropertyPanel>
      </div>
    </ui:Pane>
    
    <br /> 
    
    <ui:TabView AutoResize="false" Width="460px" Height="305px" runat="server"  ID="tv_options" />
    <ui:Pane ID="pane_content" runat="server"> 
      <div style="padding: 5px; background: #fff; height: 250px;">
        <umbraco:TreeControl runat="server" ID="TreeControl2" App="content"
                IsDialog="true" DialogMode="locallink" ShowContextMenu="false" FunctionToCall="dialogHandler"
                Height="250"></umbraco:TreeControl>
      </div>
    </ui:Pane>
    <ui:Pane ID="pane_media" runat="server">
        <div style="padding: 5px; background: #fff; height: 250px;">
            <umbraco:TreeControl runat="server" ID="TreeControl1" App="media"
                IsDialog="true" DialogMode="fulllink" ShowContextMenu="false" FunctionToCall="dialogHandler"
                Height="250"></umbraco:TreeControl>
        </div>
    </ui:Pane>
    
    <br />
    <p>
     <input type="submit" name="insert" value="{#insert}" /> <em>or</em> <a href="#" onclick="tinyMCEPopup.close();">cancel</a>
    </p>
    </form>

    

</body>
</html>
