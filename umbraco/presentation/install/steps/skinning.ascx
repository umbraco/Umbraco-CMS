<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="skinning.ascx.cs" Inherits="umbraco.presentation.install.steps.skinning" %>

<!-- Choose starter kit -->

<asp:UpdatePanel runat="server" ID="udp">
<ContentTemplate>

<script type="text/javascript">
	var intervalId = 0;

	jQuery(document).ready(function () {
		jQuery('.zoom-list a.selectStarterKit').click(function () {
			jQuery('.main-tabinfo').hide();
			jQuery('#starterkitname').html( jQuery('span', this).html() );
			
			jQuery('#single-tab1').show();
			//fire off the progressbar
			intervalId = setInterval("progressBarCallback()", 1000);
			
			
			return true;
		});
	});

	function pageLoad(sender, args) {
		if (args.get_isPartialLoad()) {

			jQuery('#single-tab1').hide();

			initZoomList2();
			initSlide();

			initLightBox();

			jQuery('.btn-install-gal').click(function () {
				jQuery('#browseSkins').hide();
				jQuery('#installingSkin').show();

				//fire off the progressbar
				intervalId = setInterval("progressBarCallback()", 1000);

				jQuery('#skinname').html(jQuery('span', this).html());
				return true;
			});
		}
	}


	function progressBarCallback() {
	    jQuery.getJSON('utills/p.aspx?feed=progress', function (data) {

	        if (data.percentage > 0) {
	            updateProgressBar(data.percentage);
	            updateStatusMessage(data.message)
	        }

	        if (data.error != "") {
	            clearInterval(intervalId);
	            updateStatusMessage(data.error);
	        }

	        if (data.percentage == 100) {
	            clearInterval(intervalId);
	            jQuery(".btn-box").show();
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
	<p>To help you get started we have created some basic starter kits each of which has been tailored to meet different requiremejnts.<br />
	Hover of the thumbnail to find out what each kit contains.</p>
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
  <p>You can now choose to skin your starter kit with one of our great designs.</p>
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


<!-- itstall skin -->
<div class="tab install-tab" id="installingSkin" style="display: none">
	<div class="container">
		<h1>Installing Skin</h1>
		<h2>Your <strong id="skinname">skin</strong> is installing. </h2>

		<div class="loader alt">
			<div class="hold">
				<div class="progress-bar"></div>
				<span class="progress-bar-value">56%</span>
			</div>
			<strong>Starting installation...</strong>
		</div>
	</div>	
</div>

<!-- lightbox -->
<div class="lightbox" id="lightbox">
	<a href="#" class="btn-close btn-close-box">close</a>
	<div class="t">&nbsp;</div>
	<div class="c">
		<div class="heading">
			<strong class="title">Name of skin</strong>
			<span class="create">Created by: <a href="#">Cogworks</a></span>
		</div>
		<div class="carusel">
			<ul>
				<li><img src="images/img09.jpg" alt="image description"></li>
				<li><img src="images/img10.jpg" alt="image description"></li>
				<li><img src="images/img11.jpg" alt="image description"></li>
			</ul>
		</div>

		<footer class="btn-box" style="display: none;">
			<a href="#single-tab4" class="single-tab btn-install btn-close-box">Install</a>
		</footer>
	</div>
	<div class="b">&nbsp;</div>
</div>