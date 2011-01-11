<%@ Control Language="c#" AutoEventWireup="True" CodeBehind="database.ascx.cs" Inherits="umbraco.presentation.install.steps.detect"
	TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<asp:PlaceHolder ID="settings" runat="server" Visible="true">
	<!-- database box -->
	<div class="tab main-tabinfo">
		<div class="container">
			<h1>Database configuration</h1>
			<p>
				<strong>To complete this step you will either need a blank database or if you do not have a blank database available, you can choose the SQL CE 4 embedded
				database (This is the recommended approach for first time users or if you are unsure).</strong></p>
            <p>
				If you are not using the SQL CE 4 embedded database you will need the connection details for your database, such as the
				&quot;connection string&quot;. You may need to contact your system administrator or web host for this information.</p>
		</div>
		<!-- database -->
		<div class="database-hold">
			<form action="#">
			<fieldset>
				<div class="step">
					<div class="container">
						<p>
							<strong>1. Do you have a blank database already installed OR do you want to use a SQL CE 4 embedded database?</strong></p>
						<ul class="mini-tabset">
							<li class="btn-yes"><a href="#database-step1" class="database-tab"><span>yes</span></a></li>
							<li class="btn-no"><a href="#database-step2" class="database-tab"><span>no</span></a></li>
						</ul>
					</div>
				</div>
				<!-- step1 -->
				<div id="database-step1">
					<div class="step">
						<div class="container">
							<p>
								<strong>2. Now choose your database type below.</strong></p>
							<div class="select">
								<asp:DropDownList runat="server" ID="DatabaseType" CssClass="sel">
									<asp:ListItem Value="" Text="Please choose" Selected="True" />
									<asp:ListItem Value="SqlServer" Text="Microsoft SQL Server" />
									<asp:ListItem Value="MySql" Text="MySQL" />
									<asp:ListItem Value="Custom" Text="Custom connection" />
									<asp:ListItem Value="SQLCE4Umbraco.SqlCEHelper,SQLCE4Umbraco" Text="SQL CE 4 (beta)" />
								</asp:DropDownList>
							</div>
						</div>
					</div>
					<div class="step">
						<div class="container">
							<p class="instructionText">
								<strong>3. Connection details:</strong> Please fill out the connection information for your database.</p>
							<div class="instruction-hold">
								<div class="row embeddedError" runat="server" id="embeddedFilesMissing" style="display: none;">
									<p>
										<strong>Missing files:</strong> SQL CE 4 requires that you manually add the SQL
										CE 4 runtime to your Umbraco installation.<br />
										You can either use the following <a href="http://our.umbraco.org/wiki/install-and-setup/using-sql-ce-4-with-juno"
											target="_blank">instructions</a> on how to add SQL CE 4 or select another database type from the dropdown above.
									</p>
								</div>

                                <div class="row embedded"  style="display: none;">
									<p>
										<strong>Nothing to configure:</strong>SQL CE 4 does not require any configuration,
                                        simply click the "install" button to continue.
									</p>
								</div>

								<asp:PlaceHolder ID="ph_dbError" runat="server" Visible="false">
									<div class="row error">
										<p class="text">
											<strong>
												<asp:Literal ID="lt_dbError" runat="server" /></strong></p>
									</div>
									<script type="text/javascript">
									  jQuery(document).ready(function () { showDatabaseSettings(); });
									</script>
								</asp:PlaceHolder>
								<div class="row sql" runat="server" id="DatabaseServerItem">
									<asp:Label runat="server" AssociatedControlID="DatabaseServer" ID="DatabaseServerLabel">Server:</asp:Label>
									<span>
										<asp:TextBox runat="server" CssClass="text" ID="DatabaseServer" /></span>
								</div>
								<div class="row sql" runat="server" id="DatabaseNameItem">
									<asp:Label runat="server" AssociatedControlID="DatabaseName" ID="DatabaseNameLabel">Database name:</asp:Label>
									<span>
										<asp:TextBox runat="server" CssClass="text" ID="DatabaseName" /></span>
								</div>
								<div class="row sql" runat="server" id="DatabaseUsernameItem">
									<asp:Label runat="server" AssociatedControlID="DatabaseUsername" ID="DatabaseUsernameLabel">Username:</asp:Label>
									<span>
										<asp:TextBox runat="server" CssClass="text" ID="DatabaseUsername" /></span>
								</div>
								<div class="row sql" runat="server" id="DatabasePasswordItem">
									<asp:Label runat="server" AssociatedControlID="DatabasePassword" ID="DatabasePasswordLabel">Password:</asp:Label>
									<span>
										<asp:TextBox runat="server" ID="DatabasePassword" CssClass="text" TextMode="Password" /></span>
								</div>
								<div class="row custom" runat="server" id="DatabaseConnectionString">
									<asp:Label runat="server" AssociatedControlID="ConnectionString" ID="ConnectionStringLabel">Connection string:</asp:Label>
									<span>
										<asp:TextBox runat="server" TextMode="MultiLine" CssClass="text" ID="ConnectionString" /></span>
								</div>
                                <div class="row custom check-hold">
                                    <p>
										Example: <tt>datalayer=MySQL;server=192.168.2.8;user id=user;password=***;database=umbraco</tt></p>
                                </div>
							</div>
						</div>
						<!-- btn box -->
						<footer class="btn-box">
							<div class="t">&nbsp;</div>
								<asp:LinkButton runat="server" class="single-tab submit btn-install" onclick="saveDBConfig"><span>install</span>    </asp:LinkButton>
							</footer>
					</div>
				</div>
				<!-- step2 -->
				<div class="step" id="database-step2">
					<div class="container">
						<p>
							<strong>2. Getting a database setup for umbraco.</strong><br />
							For first time users, we recommend you select "SQL CE 4" in the dropdown box, after clicking the "yes" button.
                            This will install an easy to use database, that does not require any additional software to use.<br />
                            Alternatively, you can install Microsoft SQL Server, which will require a bit more work to get up and running.<br />
                            We have provided a step-by-step guide in the video instructions below.
						</p>
						<span class="btn-link"><a href="http://umbraco.org/getting-started">Open video instructions</a></span>
					</div>
				</div>
			</fieldset>
			</form>
		</div>
	</div>
	<script type="text/javascript">
			var hasEmbeddedDlls = <%= HasEmbeddedDatabaseFiles.ToString().ToLower() %>;
			var currentVersion = '<%=umbraco.GlobalSettings.CurrentVersion%>';
			var configured = <%= IsConfigured.ToString().ToLower() %>;

            jQuery(document).ready(function(){
			    <asp:literal runat="server" id="jsVars" />
            });
	</script>
</asp:PlaceHolder>

<asp:PlaceHolder ID="installing" runat="server" Visible="false">
	<!-- installing umbraco -->
	<div class="tab  install-tab" id="datebase-tab">
		<div class="container">
			<h1>
				Installing Umbraco</h1>
			<p>
				The Umbraco database is being configured. This process populates your chosen database with a blank Umbraco instance.</p>
			<div class="loader">
				<div class="hold">
					<div class="progress-bar">
					</div>
					<span class="progress-bar-value">0%</span>
				</div>
				<strong></strong>
			</div>
		</div>
		<!-- btn box -->
		 <footer class="btn-box" style="display: none;">
			<div class="t">&nbsp;</div>
			 <asp:LinkButton class="btn-step btn btn-continue" runat="server" OnClick="gotoNextStep"><span>Continue</span></asp:LinkButton>
             <asp:LinkButton class="btn-step btn btn-back" style="display: none;" runat="server" OnClick="gotoSettings"><span>Back</span></asp:LinkButton>
		</footer>
	</div>

	<script type="text/javascript">
	  var intervalId = 0;

	  jQuery(document).ready(function () {
	    intervalId = setInterval("progressBarCallback()", 1000);
	    jQuery(".btn-box").hide();
	    jQuery.ajax({
	      type: 'POST',
	      contentType: 'application/json; charset=utf-8',
	      data: '{}',
	      dataType: 'json',
	      url: 'utills/p.aspx/installOrUpgrade'
	    });
	  });

	  function progressBarCallback() {
	    jQuery.getJSON('utills/p.aspx?feed=progress', function (data) {

	      updateProgressBar(data.percentage);
	      updateStatusMessage(data.message)

	      if (data.error != "") {
	        clearInterval(intervalId);
	        updateStatusMessage(data.error);

	        jQuery(".loader .hold").hide();

	        jQuery(".btn-continue").hide();
	        jQuery(".btn-back").show();
	        jQuery(".btn-box").show();
	      }

	      if (data.percentage == 100) {
	        clearInterval(intervalId);
	        jQuery(".btn-box").show();
	        jQuery('.ui-progressbar-value').css("background-image", "url(../umbraco_client/installer/images/pbar.gif)");
	      }
	    });
	  }
	</script>

</asp:PlaceHolder>

<asp:Panel ID="confirms" runat="server" Visible="False">
	<asp:PlaceHolder ID="installConfirm" runat="server" Visible="False">
		<h1>
			Database installed</h1>
		<div class="success">
			<p>
				Umbraco
				<%=umbraco.GlobalSettings.CurrentVersion%>
				has now been copied to your database. Press <b>Continue</b> to proceed.</p>
		</div>
	</asp:PlaceHolder>
	<asp:PlaceHolder ID="upgradeConfirm" runat="server" Visible="False">
		<h1>
			Database upgraded</h1>
			<div class="success">
				<p>
					Your database has been upgraded to version: 
					<%=umbraco.GlobalSettings.CurrentVersion%>.<br />
					Press <b>Continue</b> to proceed.
				</p>
			</div>
	</asp:PlaceHolder>
	<!-- btn box -->
	<footer class="btn-box" style="display: none;">
		 <div class="t">&nbsp;</div>
			<asp:LinkButton class="btn-step btn btn-continue" runat="server" OnClick="gotoNextStep"><span>Continue</span></asp:LinkButton>
		 </footer>
</asp:Panel>
