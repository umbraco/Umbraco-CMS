<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MemberPicker.ascx.cs" Inherits="Training.Level2.usercontrols.MemberPicker" %>

<asp:textbox id="tb_members" runat="server" cssclass="tokeninput" />

<script type="text/javascript">
    jQuery(document).ready(function(){
        jQuery("input.tokeninput").tokenInput("/base/Mentions/Members/", {
                                theme: "facebook",
                                prePopulate:[
                                    <asp:literal id="js_values" runat="server" />
                                ]
                            });
        });
</script>