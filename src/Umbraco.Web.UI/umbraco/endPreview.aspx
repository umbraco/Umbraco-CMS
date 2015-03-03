<%@ Page Language="C#" AutoEventWireup="true" Inherits="System.Web.UI.Page" %>

<script runat="server">

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        global::umbraco.presentation.preview.PreviewContent.ClearPreviewCookie();

        if (!Uri.IsWellFormedUriString(Request.QueryString["redir"], UriKind.Relative))
        {
            Response.Redirect("/", true);
        }
        Uri url;
        if (!Uri.TryCreate(Request.QueryString["redir"], UriKind.Relative, out url))
        {
            Response.Redirect("/", true);
        }

        Response.Redirect(url.ToString(), true);
    }

</script>