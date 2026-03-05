import type { UmbMockPartialViewModel } from '../../types/mock-data-set.types.js';
import type { PartialViewSnippetResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

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
</div>`,
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
</div>`,
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

@{ var selection = Model.Ancestors().ToArray(); }

@if (selection?.Length > 0)
{
	<ul class="breadcrumb">
		@foreach (var item in selection.OrderBy(x => x.Level))
		{
			<li><a href="@item.Url(PublishedUrlProvider)">@item.Name</a> <span class="divider">/</span></li>
		}
		<li class="active">@Model.Name</li>
	</ul>
}`,
	},
];
