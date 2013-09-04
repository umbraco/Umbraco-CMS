<%@ Page MasterPageFile="../../masterpages/umbracoDialog.Master" Language="C#" AutoEventWireup="true" CodeBehind="formPickerChooseFormDialog.aspx.cs" Inherits="Umbraco.Forms.UI.Dialogs.formPickerDialog" %>

<%@ Register TagPrefix="ui" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="server">

 <style type="text/css">
        .formselectpane
        {
            height: 425px;
            overflow: auto;
        }
        
        #header
        {
            background: #FFFFFF url(/umbraco_client/modal/modalGradiant.gif) center bottom repeat-x;
            border-bottom: 1px solid #CCC;
        }
        
        #caption
        {
            font: bold 100% "Lucida Grande" , Arial, sans-serif;
            text-shadow: #FFF 0 1px 0;
            padding: .5em 2em .5em .75em;
            margin: 0;
            text-align: left;
        }
        #close
        {
            display: block;
            position: absolute;
            right: 5px;
            top: 4px;
            padding: 2px 3px;
            font-weight: bold;
            text-decoration: none;
            font-size: 13px;
        }
        #close:hover
        {
            background: transparent;
        }
        #body
        {
            padding: 7px;
            font-size: 11px;
        }
        
        .radio{clear: left; float: left;}
        .form{float: left; clear: right; padding: 5px 0px 20px 10px; font: 14px #000 bold;}
        .form small{font-size: 10px; color: #ccc; font-weight: normal; display: block; padding-top: 3px;} 
        .form small a{display: block;}   
        
    </style>

   

    <script type="text/javascript" language="javascript">

        $(document).ready(function () {
            $("input[name='selectedform']").change(function () {
                dialogHandler($("input[name='selectedform']:checked").val());
            });

            $(".form a").click(function () {

                //window.parent.showPopWin(url, width, height, returnFunc, showCloseBox);

                var modal = jQuery("#popupContainer", window.parent.document);
                var modalFrame = jQuery("#popupFrame", window.parent.document);

                var h = 530;
                var w = 670;

                modal.height(h);
                modal.width(w);
                modalFrame.height(h);
                modalFrame.width(w);

                window.parent.centerPopWin(h, w);
                window.parent.setMaskSize();


                modalFrame.attr("src", "<%= Umbraco.Forms.Core.Configuration.Path %>/formpickerCreateFormdialog.aspx?guid=" + jQuery(this).attr('rel'));
                return false;
            });
        });

        var formguid = '';
        function dialogHandler(id) {
            formguid = id;
            jQuery("#submitbutton").attr("disabled", false);
        }

        function UpdatePicker() {
            if (formguid != '') {

               
                <% if (Umbraco.Forms.Core.CompatibilityHelper.IsVersion4dot5OrNewer){%>
                    UmbClientMgr.closeModalWindow(formguid);
                <%}else{%>
                
                parent.hidePopWin(true, formguid);

                <%}%>
            }
        }
      </script>

</asp:Content>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="body">

 
            <ui:Pane ID="dummy" runat="server" Visible="false">
            </ui:Pane>
            <div class="propertypane formselectpane">
                <div>
                    <asp:Repeater ID="Repeater1" runat="server">
                        <ItemTemplate>
                            <div class="formselect" style="padding-bottom: 10px">
                                
                                <input type="radio" name="selectedform" class="radio" value="<%# ((Umbraco.Forms.Core.Form)Container.DataItem).Id %>" id="rb_<%# ((Umbraco.Forms.Core.Form)Container.DataItem).Id %>" />
                                
                                <div class="form">
                                <label for="rb_<%# ((Umbraco.Forms.Core.Form)Container.DataItem).Id %>">
                                <%# ((Umbraco.Forms.Core.Form)Container.DataItem).Name %>
                                </label>
                                <small>
                                    <%# getFormFields(((Umbraco.Forms.Core.Form)Container.DataItem))%>

                                     <% if (!Umbraco.Forms.Core.CompatibilityHelper.IsVersion4dot5OrNewer){%>
                                    <a href="#" rel="<%# ((Umbraco.Forms.Core.Form)Container.DataItem).Id %>">Edit before inserting</a>
                                    <%}%>

                                </small>
                                
                                </div>
                                
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                    <div class="propertyPaneFooter">
                        -</div>
                </div>
            </div>
            <p>
                <input type="submit" value="insert" style="width: 60px;" disabled="true" id="submitbutton" onclick="UpdatePicker();return false;"/>
                <em id="orcopy">or</em> <a href="#" style="color: blue" onclick="<% if (Umbraco.Forms.Core.CompatibilityHelper.IsVersion4dot5OrNewer){%>UmbClientMgr.closeModalWindow()<%}else{%>parent.hidePopWin(false,0)<%}%>;"
                    id="cancelbutton">cancel</a>
            </p>



</asp:Content>


  
