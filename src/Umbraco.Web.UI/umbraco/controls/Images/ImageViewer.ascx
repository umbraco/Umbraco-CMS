<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ImageViewer.ascx.cs" Inherits="umbraco.controls.Images.ImageViewer" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<umb:JsInclude ID="JsInclude1" runat="server" FilePath="controls/Images/ImageViewer.js" PathNameAlias="UmbracoRoot" />
<div id="<%#this.ClientID%>" class="imageViewer" >

	<asp:MultiView runat="server" ActiveViewIndex='<%#(int)ViewerStyle%>'>
		<asp:View runat="server">
			<a href="<%#MediaItemPath%>" title="<%#AltText%>" target="<%#LinkTarget%>">
				<img src="<%#MediaItemThumbnailPath%>" alt="<%#AltText%>" border="0" class='<%#ImageFound ? "" : "noimage" %>' />
			</a>	
		</asp:View>
		<asp:View runat="server">       
		    <img src="<%#MediaItemThumbnailPath%>" alt="<%#AltText%>" border="0" class='<%#ImageFound ? "" : "noimage" %>' />
		</asp:View>
		<asp:View runat="server">            
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
	umbPath: "<%#umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco)%>",
		style: "<%#ViewerStyle.ToString()%>",
		linkTarget: "<%#LinkTarget%>"
};

	if (jQuery.isReady) {
		//because this may be rendered with AJAX, the doc may already be ready! so just wire it up.
		jQuery("#<%#this.ClientID%>").UmbracoImageViewer(opts);
	}
	else {
	    jQuery(document).ready(function() {
	        jQuery("#<%#this.ClientID%>").UmbracoImageViewer(opts);
		});
	} 
</script>

