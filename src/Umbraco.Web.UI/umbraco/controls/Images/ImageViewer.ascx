<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="ImageViewer.ascx.cs" Inherits="Umbraco.Web.UI.Umbraco.Controls.Images.ImageViewer" %>
<%@ Import Namespace="Umbraco.Core.IO" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<umb:JsInclude ID="JsInclude1" runat="server" FilePath="controls/Images/ImageViewer.js" PathNameAlias="UmbracoRoot" />
<div id="<%# ClientID %>" class="imageViewer" >

	<asp:MultiView ID="MultiView" runat="server">
		<asp:View ID="Basic" runat="server">
		    <img src="<%#MediaItemThumbnailPath%>" alt="<%#AltText%>" border="0" class='<%#ImageFound ? "" : "noimage" %>' />
		</asp:View>
		<asp:View ID="ImageLink" runat="server">
			<a href="<%#MediaItemPath%>" title="<%#AltText%>" target="<%#LinkTarget%>">
				<img src="<%#MediaItemThumbnailPath%>" alt="<%#AltText%>" border="0" class='<%#ImageFound ? "" : "noimage" %>' />
			</a>	
		</asp:View>
        <asp:View ID="ThumbnailPreview" runat="server">            
            <div class="bgImage" 
                style="width: 105px; height: 105px; background: #fff center center no-repeat;border: 1px solid #ccc; background-image: url('<%#MediaItemThumbnailPath.Replace(" ", "%20")%>');">
            </div>
		</asp:View>
	</asp:MultiView>

	
	<%--Register the javascript callback method if any.--%>

	<script type="text/javascript">
	    <%#string.IsNullOrEmpty(ClientCallbackMethod) ? "" : ClientCallbackMethod + "('" + MediaItemPath + "','" + AltText + "','" + FileWidth + "','" + FileHeight + "');" %>			 
	</script>
</div>
<%--Ensure that the client API is registered for the image.--%>

<script type="text/javascript">
    var opts = {
        umbPath: "<%# IOHelper.ResolveUrl(SystemDirectories.Umbraco) %>",
	    style: "<%#ViewerStyle.ToString()%>",
	    linkTarget: "<%#LinkTarget%>"
	};

	if (jQuery.isReady) {
	    //because this may be rendered with AJAX, the doc may already be ready! so just wire it up.
	    jQuery("#<%# ClientID %>").UmbracoImageViewer(opts);
	}
	else {
	    jQuery(document).ready(function () {
	        jQuery("#<%# ClientID %>").UmbracoImageViewer(opts);
	    });
    }
</script>

