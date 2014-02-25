<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="StarterKits.ascx.cs" Inherits="Umbraco.Web.UI.Install.Steps.StarterKits" %>

<!-- Choose starter kit -->

<asp:UpdatePanel runat="server" ID="udp">
<ContentTemplate>

<script type="text/javascript">
	var intervalId = 0;

	jQuery(document).ready(function () {
		jQuery('.zoom-list a.selectStarterKit').click(function () {
			jQuery('.main-tabinfo').hide();
			jQuery('#starterkitname').html( jQuery(this).attr("title") );
			
			jQuery('#single-tab1').show();
			//fire off the progressbar
			intervalId = setInterval("progressBarCallback()", 1000);
			
			
			return true;
		});
	});

	function pageLoad(sender, args) {
		if (args.get_isPartialLoad()) {

		    clearInterval(intervalId);

			jQuery('#single-tab1').hide();

			initZoomList2();
			initSlide();

			initLightBox();

			jQuery('.btn-install-gal').click(function () {
			    jQuery('#browseSkins').hide();
			    jQuery('#installingSkin').show();

			    //fire off the progressbar
			    intervalId = setInterval("progressBarCallback()", 1000);

			    jQuery('#skinname').html( jQuery(this).attr("title") );
			    return true;
			});
		}
	}


	function progressBarCallback() {
	    jQuery.getJSON('InstallerRestService.aspx?feed=progress', function (data) {

			if (data.percentage > 0) {
				updateProgressBar(data.percentage);
			    updateStatusMessage(data.message);
			}

			if (data.error != "") {
				clearInterval(intervalId);
				updateStatusMessage(data.error);
			}

			if (data.percentage == 100) {
				clearInterval(intervalId);
				jQuery(".btn-box").show();
				jQuery('.ui-progressbar-value').css("background-image", "url(../umbraco_client/installer/images/pbar.gif)");
			}
		});
	}
	</script>
</script>



<asp:Placeholder ID="pl_starterKit" Runat="server" Visible="True">
<!-- starter box -->
<div class="tab main-tabinfo">
<div class="container">
	<h1>Starter kits</h1>
	<p>To help you get started here are some basic starter kits. They have been tailored to suit common site configurations and install useful functionality.</p>
</div>
<!-- menu -->
<asp:PlaceHolder ID="ph_starterKits" runat="server" />
</div>
</asp:Placeholder>


<!-- Choose starter kit design -->
<asp:Placeholder ID="pl_starterKitDesign" Runat="server" Visible="True">
<div class="tab install-tab" id="browseSkins">
<div class="container">
<h1>Install a Skin</h1>
<div class="accept-hold">
  <p>You can now further enhance your site by choosing one of these great skins. This will apply a default look and feel to all the pages in your site, considerably reducing development time.</p>
</div>
</div>
<!-- skins -->
<asp:Placeholder ID="ph_starterKitDesigns" runat="server" />
</div>
</asp:Placeholder>

</ContentTemplate>
</asp:UpdatePanel>



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
			<strong>Starting installation...</strong>
		</div>
	</div>
</div>

