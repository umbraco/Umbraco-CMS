<%@ Page Language="c#" MasterPageFile="../masterpages/umbracoPage.Master" ValidateRequest="false"
    CodeBehind="editMacro.aspx.cs" AutoEventWireup="True" Inherits="umbraco.dialogs.editMacro"
    Trace="false" %>

<%@ Register TagPrefix="cc2" Namespace="umbraco.uicontrols" Assembly="controls" %>
<asp:Content ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
	  function saveTreepickerValue(appAlias, macroAlias) {
				var treePicker = window.showModalDialog('treePicker.aspx?app=' + appAlias + '&treeType=' + appAlias, 'treePicker', 'dialogWidth=350px;dialogHeight=300px;scrollbars=no;center=yes;border=thin;help=no;status=no')
				document.forms[0][macroAlias].value = treePicker;
				document.getElementById("label" + macroAlias).innerHTML = "</b><i>updated with id: " + treePicker + "</i><b><br/>";
		  }
			
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
                if (control == null || (!control.is('input') && !control.is('select') && !control.is('textarea'))) {
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
     
			UmbClientMgr.contentFrame().focus();
			UmbClientMgr.contentFrame().UmbEditor.Insert(macroString, '', '<%=umbraco.helper.Request("objectId")%>');			
			UmbClientMgr.closeModalWindow();
		}

		function pseudoHtmlEncode(text) {
			return text.replace(/\"/gi,"&amp;quot;").replace(/\</gi,"&amp;lt;").replace(/\>/gi,"&amp;gt;");
		}
    </script>
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
</asp:Content>
<asp:Content ContentPlaceHolderID="body" runat="server">
    <asp:Panel ID="pl_edit" runat="server" Visible="false">
        <cc2:Pane ID="pane_edit" runat="server">
            <div style="height: 420px; overflow: auto;">
                <asp:PlaceHolder ID="macroProperties" runat="server" />
            </div>
        </cc2:Pane>
        <p>
            <input type="button" value="<%=umbraco.ui.Text("general", "ok", this.getUser())%>"
                onclick="updateMacro()" />
            <em>or </em><a href="#" style="color: blue" onclick="UmbClientMgr.closeModalWindow()">
                <%=umbraco.ui.Text("general", "cancel", this.getUser())%></a>
        </p>
    </asp:Panel>
    <asp:Panel ID="pl_insert" runat="server">
        <cc2:Pane ID="pane_insert" runat="server">
            <cc2:PropertyPanel ID="pp_chooseMacro" runat="server" Text="Choose a macro">
                <asp:ListBox Rows="1" ID="umb_macroAlias" Width="200px" runat="server"></asp:ListBox>
            </cc2:PropertyPanel>
        </cc2:Pane>
        <p>
            <asp:Button ID="bt_insert" runat="server" Text="ok" OnClick="renderProperties"></asp:Button>
            <em>or </em><a href="#" style="color: blue" onclick="UmbClientMgr.closeModalWindow()">
                <%=umbraco.ui.Text("general", "cancel", this.getUser())%></a>
        </p>
    </asp:Panel>
    <div id="renderContent" style="display: none">
        <asp:PlaceHolder ID="renderHolder" runat="server"></asp:PlaceHolder>
    </div>
</asp:Content>
