<%@ Page language="c#" Codebehind="insertImage.aspx.cs" AutoEventWireup="True" Inherits="umbraco.dialogs.insertImage" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<HEAD>
		<title>umbraco ::
			<%=umbraco.ui.Text("defaultdialogs", "insertimage", this.getUser())%>
		</title>
		<meta name="GENERATOR" Content="Microsoft Visual Studio .NET 7.1">
		<meta name="CODE_LANGUAGE" Content="C#">
		<meta name="vs_defaultClientScript" content="JavaScript">
		<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
		<LINK href="../css/umbracoGui.css" type="text/css" rel="stylesheet">
		<style>BODY { MARGIN: 2px }
			label {
				width:100px;
			}
		</style>
		<script language="javascript">
			function dialogHandler(id) {
				document.frames.imageViewer.document.location.href = 'dialogs/imageViewer.aspx?id=' + id;
			}
			
			function updateImageSource(src, alt, width, height) {
				document.getElementById("imageSrc").value = src;
				document.getElementById("imageAlt").value = alt;
				document.getElementById("imageWidth").value = width;
				document.getElementById("imageHeight").value = height;
			}
			
			function insertImage()
			{
				var imageName = document.getElementById("imageAlt").value;
				var imageWidth = document.getElementById("imageWidth").value;
				var imageHeight = document.getElementById("imageHeight").value;
				var imageSource = document.getElementById("imageSrc").value.replace("thumb.jpg", ".jpg");
				var imageTitle = document.getElementById("imageTitle").value;
				var orgHeight = imageHeight;
				var orgWidth = imageWidth;
				
				if (imageWidth != '' && imageWidth > 500) {						
						if (imageWidth > imageHeight)
							orgRatio = parseFloat(imageHeight/imageWidth).toFixed(2)
						else
							orgRatio = parseFloat(imageWidth/imageHeight).toFixed(2)
						imageHeight = Math.round(500 * parseFloat(imageHeight/imageWidth).toFixed(2));
						imageWidth = 500;												
				}
				window.returnValue = imageName + '|||' + imageSource + '|||' + imageWidth + '|||' + imageHeight + '|||' + imageTitle + '|||' + orgWidth + '|||' + orgHeight;
				window.close();
			}


function doSubmit() {}

	var functionsFrame = this;
	var tabFrame = this;
	var isDialog = true;
	var submitOnEnter = true;

		</script>
		<script type="text/javascript" src="../js/umbracoCheckKeys.js"></script>
	</HEAD>
	<body MS_POSITIONING="GridLayout">
		<h3><%=umbraco.ui.Text("defaultdialogs", "insertimage",this.getUser())%></h3>
		<form id="Form1" method="post" runat="server">
			<input type="hidden" name="imageHeight"> <input type="hidden" name="imageWidth">
			<input type="hidden" name="imageSrc">
			<TABLE class="propertyPane" cellSpacing="0" cellPadding="4" width="98%" border="0" runat="server" ID="Table1">
				<TBODY>
					
					<tr>
						<TD class="propertyHeader" width="30%">
							<asp:PlaceHolder ID="PlaceHolder1" Runat="server" />
						</TD>
					</tr>
					
				</TBODY>
			</TABLE>
			<TABLE class="propertyPane" cellSpacing="0" cellPadding="4" width="98%" border="0" runat="server" ID="Table2">
				<TBODY>
				<tr>
						<td>
						<label for="imageAlt"><%=umbraco.ui.Text("alternativeText")%>:</label> <input type="text" id="imageAlt" name="imageAlt" value="" style="width:150px"/> <small><%=umbraco.ui.Text("alternativeTextHelp")%></small>
						<br />
						<label for="imageTitle"><%=umbraco.ui.Text("titleOptional")%></label> <input type="text" name="imageTitle" value="" style="width:150px"/>	
						</td>
					</tr>
					<tr>
					<td>
					<input type="button" onClick="insertImage();" value="<%=umbraco.ui.Text("defaultdialogs", "insertimage", this.getUser())%>"/>
					</td>
					</tr>
				</tbody>
			</table>
			
		</form>
	</body>
</HTML>
