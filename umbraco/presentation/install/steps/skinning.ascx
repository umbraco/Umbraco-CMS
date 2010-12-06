<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="skinning.ascx.cs" Inherits="umbraco.presentation.install.steps.skinning" %>

<!-- Choose starter kit -->

<asp:UpdatePanel runat="server" ID="udp">
<ContentTemplate>

<script type="text/javascript">
    jQuery(document).ready(function () {
        jQuery('.zoom-list a.selectStarterKit').click(function () {
            jQuery('.tab main-tabinfo').hide();
            jQuery('#starterkitname').html(jQuery('span', this).html());
            jQuery('#single-tab1').show();
            return true;
        });

        jQuery('.btn-install-gal').click(function () {
            jQuery('#browseSkins').hide();
            jQuery('#skinname').html(jQuery('span', this).html());
            jQuery('#installingSkin').show();

            return true;
        });
    });

    function pageLoad(sender, args) {
        if (args.get_isPartialLoad()) {
            initZoomList2();
            initSlide();
        }
    }
</script>


<asp:Placeholder ID="pl_starterKit" Runat="server" Visible="True">
<!-- starter box -->
<div class="tab main-tabinfo">
<div class="container">
	<h1>Starter kits</h1>
	<p>To help you get started we have created some basic starter kits each of which has been tailored to meet different requiremejnts.<br /> Hover of the thumbnail to find out what each kit contains.</p>
</div>
<!-- menu -->
<asp:PlaceHolder ID="ph_starterKits" runat="server" />
</div>

<!-- itstall starter kit -->
<div class="tab install-tab" id="single-tab1" style="display: none">
	<div class="container">
		<h1>Installing Starter Kit</h1>
		<h2><strong id="starterkitname">Your starter kit</strong> is installing. </h2>
		<div class="loader alt">
			<div class="hold">
				<div class="progress-bar"></div>
				<span class="progress-bar-value">56%</span>
			</div>
			<strong>Installation complete</strong>
		</div>
	</div>
	<!-- btn box -->
	<footer class="btn-box">
		<div class="t">&nbsp;</div>
		<a href="#single-tab2" class="single-tab btn btn-continue"><span>Continue</span></a>
	</footer>
</div>
</asp:Placeholder>


<!-- Choose starter kit design -->
<asp:Placeholder ID="pl_starterKitDesign" Runat="server" Visible="True">
<div class="tab install-tab" id="browseSkins">
<div class="container">
<h1>Install a Skin</h1>
<div class="accept-hold">
  <p>You can now choose to skin your starter kit with one of our great designs.</p>
</div>
</div>
<!-- skins -->
<asp:PlaceHolder ID="ph_starterKitDesigns" runat="server" />
</div>

<div class="tab install-tab" id="installingSkin" style="display: none">
    <div class="container">
		<h1>Installing Skin</h1>
		<h2>Your <strong id="skinname">skin</strong> is installing. </h2>

		<div class="loader alt">
			<div class="hold">
				<div class="progress-bar"></div>
				<span class="progress-bar-value">56%</span>
			</div>
			<strong>Installation complete</strong>
		</div>
	</div>
	<footer class="btn-box">
		<div class="t">&nbsp;</div>
		<a href="youre-done.html" class="btn btn-continue"><span>Continue</span></a>
	</footer>
</div>
</asp:Placeholder>

</ContentTemplate>
</asp:UpdatePanel>


<!-- Customize skin -->

<asp:Panel ID="pl_customizeDesign" Runat="server" Visible="True">

<div id="customizeSkin">
<asp:PlaceHolder ID="ph_customizeDesig" runat="server" />
</div>

</asp:Panel>

