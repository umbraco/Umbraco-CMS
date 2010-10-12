<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ModuleInjector.aspx.cs" Inherits="umbraco.presentation.umbraco.LiveEditing.Modules.SkinModule.ModuleInjector" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>

        <cc1:UmbracoClientDependencyLoader runat="server" id="ClientLoader" />
        <umb:JsInclude ID="JsInclude1" runat="server" FilePath="ui/jquery.js" PathNameAlias="UmbracoClient"
        Priority="0" />


        <umb:CssInclude ID="CssInclude1" runat="server" FilePath="ui/default.css" PathNameAlias="UmbracoClient" />
        <umb:CssInclude ID="CssInclude2" runat="server" FilePath="modal/style.css" PathNameAlias="UmbracoClient" />
         <umb:CssInclude ID="CssInclude3" runat="server" FilePath="propertypane/style.css" PathNameAlias="UmbracoClient" />
    
        <style type="text/css">
        .propertyItemheader
        {
            width: 170px !important;
        }
        
        .guiInputTextStandard
        {
            width: 220px;
        }
    </style>
    
    <script type="text/javascript">


		var macroAliases = new Array();
		var macroAlias = '<%= _macroAlias %>';
			
		<%if (umbraco.UmbracoSettings.UseAspNetMasterPages) { %>
		var macroElement = "umbraco:Macro";
		<%}else{ %>
		var macroElement = "?UMBRACO_MACRO";
		<%}%>
						
		function registerAlias(alias, pAlias) {
			var macro = new Array();
			macro[0] = alias;
			macro[1] = pAlias;
			  
			macroAliases[macroAliases.length] = macro;
		}

		  function updateMacro() {
			  var macroString = '<' + macroElement + ' ';
			
			  for (i=0; i<macroAliases.length; i++) {
				  var controlId = macroAliases[i][0];
				  var propertyName = macroAliases[i][1];
				  
					
                var control = jQuery("#" + controlId); 
                if (control == null || (!control.is('input') && !control.is('select'))) {
                    // hack for tree based macro parameter types
                    var picker = Umbraco.Controls.TreePicker.GetPickerById(controlId);
                    if (picker != undefined) {
    						macroString += propertyName + "=\"" + picker.GetValue() + "\" ";
                    }
                } else {
					if (control.is(':checkbox')) {
						if (control.is(':checked'))
							macroString += propertyName + "=\"1\" ";
						else
							macroString += propertyName + "=\"0\" ";

					} else if (control[0].tagName.toLowerCase() == 'select') {
						var tempValue = '';
						control.find(':selected').each(function(i, selected) {
							tempValue += jQuery(this).attr('value') + ', ';
                        });
/*
						for (var j=0; j<document.forms[0][controlId].length;j++) {
							if (document.forms[0][controlId][j].selected)
								tempValue += document.forms[0][controlId][j].value + ', ';
    					}
*/					
					    if (tempValue.length > 2)
							    tempValue = tempValue.substring(0, tempValue.length-2)
						
						macroString += propertyName + "=\"" + tempValue + "\" ";
					
					}else	{
						macroString += propertyName + "=\"" + pseudoHtmlEncode(document.forms[0][controlId].value) + "\" ";
					}
                }
			}
			
			if (macroString.length > 1)
				macroString = macroString.substr(0, macroString.length-1);
		
			<%if (!umbraco.UmbracoSettings.UseAspNetMasterPages){ %>
			macroString += " macroAlias=\"" + macroAlias + "\"";
			<%} %>				
				
			<%if (umbraco.UmbracoSettings.UseAspNetMasterPages){ %>
			  macroString += " Alias=\"" + macroAlias + "\" runat=\"server\"></" + macroElement + ">";
			<%} else { %>
			  macroString += "></" + macroElement + ">";
			<%} %>
     

            top.jQuery('.umbModalBoxIframe').closest(".umbModalBox").ModalWindowAPI().close();

            top.umbInsertModule('<%=umbraco.helper.Request("target")%>',macroString,'<%=umbraco.helper.Request("type")%>');
		}

		function pseudoHtmlEncode(text) {
			return text.replace(/\"/gi,"&amp;quot;").replace(/\</gi,"&amp;lt;").replace(/\>/gi,"&amp;gt;");
		}

    </script>
</head>
<body>
    <form id="form1" runat="server">

    
        <div style="" class="propertypane">
            <div>
       <div style="height: 420px; overflow: auto;">
                <asp:PlaceHolder ID="macroProperties" runat="server" />
       </div>
        <div class="propertyPaneFooter">-</div>
       </div>
      
       </div>

       <p>
       <input type="button" value="<%=umbraco.ui.Text("general", "ok", this.getUser())%>"
                onclick="updateMacro()" />
        </p>
    </form>
</body>
</html>
