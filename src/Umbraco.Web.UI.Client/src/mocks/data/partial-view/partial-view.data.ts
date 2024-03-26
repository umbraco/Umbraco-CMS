import type {
	FileSystemTreeItemPresentationModel,
	PartialViewItemResponseModel,
	PartialViewResponseModel,
	PartialViewSnippetResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockPartialViewModel = PartialViewResponseModel &
	FileSystemTreeItemPresentationModel &
	PartialViewItemResponseModel;

export const data: Array<UmbMockPartialViewModel> = [
	{
		name: 'blockgrid',
		path: '/blockgrid',
		parent: null,
		isFolder: true,
		hasChildren: true,
		content: '',
	},
	{
		name: 'blocklist',
		path: '/blocklist',
		parent: null,
		isFolder: true,
		hasChildren: true,
		content: '',
	},
	{
		name: 'grid',
		path: '/grid',
		parent: null,
		isFolder: true,
		hasChildren: true,
		content: '',
	},
	{
		name: 'area.cshtml',
		path: '/blockgrid/area.cshtml',
		parent: {
			path: '/blockgrid',
		},
		isFolder: false,
		hasChildren: false,
		content: `@using Umbraco.Extensions
		@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<Umbraco.Cms.Core.Models.Blocks.BlockGridArea>
		
		<div class="umb-block-grid__area"
			 data-area-col-span="@Model.ColumnSpan"
			 data-area-row-span="@Model.RowSpan"
			 data-area-alias="@Model.Alias"
			 style="--umb-block-grid--grid-columns: @Model.ColumnSpan;--umb-block-grid--area-column-span: @Model.ColumnSpan; --umb-block-grid--area-row-span: @Model.RowSpan;">
			@await Html.GetBlockGridItemsHtmlAsync(Model)
		</div>
		`,
	},
	{
		name: 'items.cshtml',
		path: '/blockgrid/items.cshtml',
		parent: {
			path: '/blockgrid',
		},
		isFolder: false,
		hasChildren: false,
		content: '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage',
	},
	{
		name: 'default.cshtml',
		path: '/blocklist/default.cshtml',
		parent: {
			path: '/blocklist',
		},
		isFolder: false,
		hasChildren: false,
		content: `@using Umbraco.Extensions
		@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<Umbraco.Cms.Core.Models.Blocks.BlockGridModel>
		@{
			if (Model?.Any() != true) { return; }
		}
		
		<div class="umb-block-grid"
			 data-grid-columns="@(Model.GridColumns?.ToString() ?? "12");"
			 style="--umb-block-grid--grid-columns: @(Model.GridColumns?.ToString() ?? "12");">
			@await Html.GetBlockGridItemsHtmlAsync(Model)
		</div>
		`,
	},
	{
		name: 'embed.cshtml',
		path: '/grid/embed.cshtml',
		parent: {
			path: '/grid',
		},
		isFolder: false,
		hasChildren: false,
		content: `@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<dynamic>

		@if (Model is not null)
		{
		  string embedValue = Convert.ToString(Model.value);
		  embedValue = embedValue.DetectIsJson() ? Model.value.preview : Model.value;
		
		  <div class="video-wrapper">
			@Html.Raw(embedValue)
		  </div>
		}
		`,
	},
	{
		name: 'default.cshtml',
		path: '/grid/default.cshtml',
		parent: {
			path: '/grid',
		},
		isFolder: false,
		hasChildren: false,
		content: '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage',
	},
];

export const snippets: Array<PartialViewSnippetResponseModel> = [
	{
		name: 'Empty',
		id: '37f8786b-0b9b-466f-97b6-e736126fc545',
		content: '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage',
	},
	{
		name: 'Breadcrumb',
		id: '4ed59952-d0aa-4583-9c3d-9f6b7068dcea',
		content: `@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage
		@using Umbraco.Cms.Core.Routing
		@using Umbraco.Extensions
		
		@inject IPublishedUrlProvider PublishedUrlProvider
		@*
			This snippet makes a breadcrumb of parents using an unordered HTML list.
		
			How it works:
			- It uses the Ancestors() method to get all parents and then generates links so the visitor can go back
			- Finally it outputs the name of the current page (without a link)
		*@
		
		@{ var selection = Model.Ancestors().ToArray(); }
		
		@if (selection?.Length > 0)
		{
			<ul class="breadcrumb">
				@* For each page in the ancestors collection which have been ordered by Level (so we start with the highest top node first) *@
				@foreach (var item in selection.OrderBy(x => x.Level))
				{
					<li><a href="@item.Url(PublishedUrlProvider)">@item.Name</a> <span class="divider">/</span></li>
				}
		
				@* Display the current page as the last item in the list *@
				<li class="active">@Model.Name</li>
			</ul>
		}`,
	},
	{
		name: 'EditProfile',
		id: 'a3a41f2d-0891-49f6-b2a1-d75e342fe14e',
		content: `@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage

		@using Umbraco.Cms.Core.Services
		@using Umbraco.Cms.Web.Common.Security
		@using Umbraco.Cms.Web.Website.Controllers
		@using Umbraco.Cms.Web.Website.Models
		@using Umbraco.Extensions
		@inject MemberModelBuilderFactory memberModelBuilderFactory;
		@inject IMemberExternalLoginProviders memberExternalLoginProviders
		@inject IExternalLoginWithKeyService externalLoginWithKeyService
		@{
		
			// Build a profile model to edit
			var profileModel = await memberModelBuilderFactory
				.CreateProfileModel()
				// If null or not set, this will redirect to the current page
				.WithRedirectUrl(null)
				// Include editable custom properties on the form
				.WithCustomProperties(true)
				.BuildForCurrentMemberAsync();
		
			var success = TempData["FormSuccess"] != null;
		
			var loginProviders = await memberExternalLoginProviders.GetMemberProvidersAsync();
			var externalSignInError = ViewData.GetExternalSignInProviderErrors();
		
			var currentExternalLogin = profileModel is null
				? new Dictionary<string, string>()
				: externalLoginWithKeyService.GetExternalLogins(profileModel.Key).ToDictionary(x=>x.LoginProvider, x=>x.ProviderKey);
		}
		
		<script src="https://cdnjs.cloudflare.com/ajax/libs/jquery/3.4.1/jquery.min.js"></script>
		<script src="https://cdnjs.cloudflare.com/ajax/libs/jquery-validate/1.19.0/jquery.validate.min.js"></script>
		<script src="https://cdnjs.cloudflare.com/ajax/libs/jquery-validation-unobtrusive/3.2.11/jquery.validate.unobtrusive.min.js"></script>
		
		@if (profileModel != null)
		{
			if (success)
			{
				@* This message will show if profileModel.RedirectUrl is not defined (default) *@
				<p class="text-success">Profile updated</p>
			}
		
			using (Html.BeginUmbracoForm<UmbProfileController>("HandleUpdateProfile", new { RedirectUrl = profileModel.RedirectUrl }))
			{
				<h2>Update your account.</h2>
				<hr />
				<div asp-validation-summary="All" class="text-danger"></div>
				<div class="mb-3">
					<label asp-for="@profileModel.Name" class="form-label"></label>
					<input asp-for="@profileModel.Name" class="form-control" aria-required="true" />
					<span asp-validation-for="@profileModel.Name" class="form-text text-danger"></span>
				</div>
				<div class="mb-3">
					<label asp-for="@profileModel.Email" class="form-label"></label>
					<input asp-for="@profileModel.Email" class="form-control" autocomplete="username" aria-required="true" />
					<span asp-validation-for="@profileModel.Email" class="form-text text-danger"></span>
				</div>
		
				@if (!string.IsNullOrWhiteSpace(profileModel.UserName))
				{
					<div class="mb-3">
						<label asp-for="@profileModel.UserName" class="form-label"></label>
						<input asp-for="@profileModel.UserName" class="form-control" autocomplete="username" aria-required="true" />
						<span asp-validation-for="@profileModel.UserName" class="form-text text-danger"></span>
					</div>
				}
		
				@if (profileModel.MemberProperties != null)
				{
					for (var i = 0; i < profileModel.MemberProperties.Count; i++)
					{
						<div class="mb-3">
							@Html.LabelFor(m => profileModel.MemberProperties[i].Value, profileModel.MemberProperties[i].Name)
							<input asp-for="@profileModel.MemberProperties[i].Value" class="form-control" />
							@Html.HiddenFor(m => profileModel.MemberProperties[i].Alias)
							<span asp-validation-for="@profileModel.MemberProperties[i].Value" class="form-text text-danger"></span>
						</div>
					}
				}
		
				<button type="submit" class="btn btn-primary">Update</button>
		
				if (loginProviders.Any())
				{
					<hr/>
					<h4>Link external accounts</h4>
		
					if (externalSignInError?.AuthenticationType is null && externalSignInError?.Errors.Any() == true)
					{
						@Html.DisplayFor(x => externalSignInError.Errors);
					}
		
					@foreach (var login in loginProviders)
					{
						if (currentExternalLogin.TryGetValue(login.ExternalLoginProvider.AuthenticationType, out var providerKey))
						{
							@using (Html.BeginUmbracoForm<UmbExternalLoginController>(nameof(UmbExternalLoginController.Disassociate)))
							{
								<input type="hidden" name="providerKey" value="@providerKey"/>
								<button type="submit" name="provider" value="@login.ExternalLoginProvider.AuthenticationType">
									Un-Link your @login.AuthenticationScheme.DisplayName account
								</button>
		
								if (externalSignInError?.AuthenticationType == login.ExternalLoginProvider.AuthenticationType)
								{
									@Html.DisplayFor(x => externalSignInError.Errors);
								}
							}
						}
						else
						{
							@using (Html.BeginUmbracoForm<UmbExternalLoginController>(nameof(UmbExternalLoginController.LinkLogin)))
							{
								<button type="submit" name="provider" value="@login.ExternalLoginProvider.AuthenticationType">
									Link your @login.AuthenticationScheme.DisplayName account
								</button>
		
								if (externalSignInError?.AuthenticationType == login.ExternalLoginProvider.AuthenticationType)
								{
									@Html.DisplayFor(x => externalSignInError.Errors);
								}
							}
						}
		
					}
				}
			}
		}`,
	},
	{
		name: 'Login',
		id: 'd5a4f7fb-ca4f-4452-9071-0795db207103',
		content: 'login',
	},
	{
		name: 'Login Status',
		id: '5d0683c8-50b4-4f54-817f-60992ca2a92a',
		content: 'loginStatus',
	},
	{
		name: 'Multinode Tree-picker',
		id: 'c9e264c6-6281-4c94-a7cb-de2fb7c81c04',
		content: 'MultinodeTree-picker',
	},
	{
		name: 'Navigation',
		id: '1b836d87-969b-4f58-807e-c959a1175c65',
		content: 'Navigation',
	},
	{
		name: 'Register Member',
		id: '2f49f869-e382-4e7b-baa4-9da5cfbb134b',
		content: 'RegisterMember',
	},
	{
		name: 'Site Map',
		id: 'e4a8577a-abf6-4bb1-a942-2a99b10558b4',
		content: 'SiteMap',
	},
];
