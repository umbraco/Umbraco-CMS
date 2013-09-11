<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UpgradeReport.ascx.cs" Inherits="Umbraco.Web.UI.Install.Steps.UpgradeReport" %>

<div class="tab main-tabinfo">
    <div class="container">
        <h1>Major version upgrade from <%= CurrentVersion %> to <%= NewVersion %></h1>

        <asp:MultiView runat="server" ActiveViewIndex="<%#ToggleView.ActiveViewIndex %>" ID="MultiView1">
            <asp:View ID="View1" runat="server">
                <p>
                    This installation step will determine if there are compatibility issues with Property Editors that you have defined in your current installation.
                </p>
            </asp:View>
            <asp:View ID="View2" runat="server">

                <asp:MultiView runat="server" ActiveViewIndex="<%#Report.Any() ? 0 : 1 %>">
                    <asp:View runat="server">
                        <h2>There were <%=Report.Count() %> issues detected</h2>
                        <p>
                            The following compatibility issues were found. If you continue all non-compatible property editors will be converted to a Readonly/Label.                             
                            You will be able to change the property editor to a compatible type manually by editing the data type after installation.
                        </p>
                        <p>
                            Otherwise if you choose not to proceed you will need to fix the errors listed below. 
                            Refer to v<%= NewVersion%> upgrade instructions for full details.
                        </p>
                    </asp:View>
                    <asp:View runat="server">
                        <h2>No issues detected</h2>
                        <p>
                            <strong>Click 'Continue' to proceed with the upgrade</strong>
                        </p>
                    </asp:View>
                </asp:MultiView>
            </asp:View>
        </asp:MultiView>

    </div>
    <div class="step rendering-engine">
        <div class="container btn-box">
            <asp:MultiView runat="server" ActiveViewIndex="0" ID="ToggleView">
                <asp:View runat="server">
                    <p>
                        <strong>Click 'Continue' to generate the compatibility report</strong>
                    </p>
                </asp:View>
                <asp:View runat="server">

                    <table class="upgrade-report">
                        <% foreach (var item in Report)
                           { %>

                        <tr>
                            <td class="icon">
                                <span class='<%= item.Item1 ? "ui-state-default ui-icon ui-icon-check" : "ui-state-highlight ui-icon ui-icon-alert" %>'></span>
                            </td>
                            <td class="msg">
                                <%=item.Item2 %>    
                            </td>
                        </tr>

                        <% } %>
                    </table>
                </asp:View>
            </asp:MultiView>

        </div>
    </div>
    <!-- btn box -->
    <footer class="btn-box">
        <div class="t">&nbsp;</div>
        <asp:LinkButton ID="btnNext" CssClass="btn btn-continue" runat="server" OnClick="NextButtonClick"><span>Continue</span></asp:LinkButton>
    </footer>
</div>
