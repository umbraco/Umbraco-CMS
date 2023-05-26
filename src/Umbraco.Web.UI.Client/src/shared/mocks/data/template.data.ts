import { UmbEntityData } from './entity.data';
import { createEntityTreeItem } from './utils';
import { UmbId } from '@umbraco-cms/backoffice/id';
import {
	EntityTreeItemResponseModel,
	PagedEntityTreeItemResponseModel,
	TemplateResponseModel,
	TemplateScaffoldResponseModel,
	CreateTemplateRequestModel,
	TemplateItemResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

type TemplateDBItem = TemplateResponseModel & EntityTreeItemResponseModel;

const createTemplate = (dbItem: TemplateDBItem): TemplateResponseModel => {
	return {
		$type: '',
		id: dbItem.id,
		name: dbItem.name,
		alias: dbItem.alias,
		content: dbItem.content,
	};
};

const createTemplateItem = (dbItem: TemplateDBItem): TemplateItemResponseModel => ({
	name: dbItem.name,
	id: dbItem.id,
});

export const data: Array<TemplateDBItem> = [
	{
		$type: '',
		id: '2bf464b6-3aca-4388-b043-4eb439cc2643',
		isContainer: false,
		parentId: null,
		name: 'Doc 1',
		type: 'template',
		icon: 'umb:layout',
		hasChildren: false,
		alias: 'Doc1',
		content: `@using Umbraco.Extensions
		@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<Umbraco.Cms.Core.Models.Blocks.BlockGridItem>
		@{
			if (Model?.Areas.Any() != true) { return; }
		}

		<div class="umb-block-grid__area-container"
			 style="--umb-block-grid--area-grid-columns: @(Model.AreaGridColumns?.ToString() ?? Model.GridColumns?.ToString() ?? "12");">
			@foreach (var area in Model.Areas)
			{
				@await Html.GetBlockGridItemAreaHtmlAsync(area)
			}
		</div>`,
	},
	{
		$type: '',
		id: '9a84c0b3-03b4-4dd4-84ac-706740ac0f71',
		isContainer: false,
		parentId: null,
		name: 'Test',
		type: 'template',
		icon: 'umb:layout',
		hasChildren: true,
		alias: 'Test',
		content:
			'@using Umbraco.Cms.Web.Common.PublishedModels;\n@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<ContentModels.Test>\r\n@using ContentModels = Umbraco.Cms.Web.Common.PublishedModels;\r\n@{\r\n\tLayout = null;\r\n}',
	},
	{
		$type: '',
		id: '9a84c0b3-03b4-4dd4-84ac-706740ac0f72',
		isContainer: false,
		parentId: '9a84c0b3-03b4-4dd4-84ac-706740ac0f71',
		name: 'Child',
		type: 'template',
		icon: 'umb:layout',
		hasChildren: false,
		alias: 'Test',
		content:
			'@using Umbraco.Cms.Web.Common.PublishedModels;\n@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<ContentModels.Test>\r\n@using ContentModels = Umbraco.Cms.Web.Common.PublishedModels;\r\n@{\r\n\tLayout = null;\r\n}',
	},
	{
		$type: '',
		id: '9a84c0b3-03b4-4dd4-84ac-706740acwerer0f72',
		isContainer: false,
		parentId: '9a84c0b3-03b4-4dd4-84ac-706740ac0f71',
		name: 'Has Master Template',
		type: 'template',
		icon: 'umb:layout',
		hasChildren: false,
		alias: 'hasMasterTemplate',
		content:
			'@using Umbraco.Cms.Web.Common.PublishedModels;\n@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<ContentModels.Test>\r\n@using ContentModels = Umbraco.Cms.Web.Common.PublishedModels;\r\n@{\r\n\tLayout = "some/path/to/a/template.cshtml";\r\n}',
	},
];

export const createTemplateScaffold = (masterTemplateAlias: string) => {
	return `Template Scaffold Mock for master template: ${masterTemplateAlias}`;
};

export const templateQuerySettings = {
	contentTypeAliases: [],
	properties: [
		{
			alias: 'Id',
			type: 'Integer',
		},
		{
			alias: 'Name',
			type: 'String',
		},
		{
			alias: 'CreateDate',
			type: 'DateTime',
		},
		{
			alias: 'UpdateDate',
			type: 'DateTime',
		},
	],
	operators: [
		{
			operator: 'Equals',
			applicableTypes: ['Integer', 'String'],
		},
		{
			operator: 'NotEquals',
			applicableTypes: ['Integer', 'String'],
		},
		{
			operator: 'LessThan',
			applicableTypes: ['Integer', 'DateTime'],
		},
		{
			operator: 'LessThanEqualTo',
			applicableTypes: ['Integer', 'DateTime'],
		},
		{
			operator: 'GreaterThan',
			applicableTypes: ['Integer', 'DateTime'],
		},
		{
			operator: 'GreaterThanEqualTo',
			applicableTypes: ['Integer', 'DateTime'],
		},
		{
			operator: 'Contains',
			applicableTypes: ['String'],
		},
		{
			operator: 'NotContains',
			applicableTypes: ['String'],
		},
	],
};

// Temp mocked database
class UmbTemplateData extends UmbEntityData<TemplateDBItem> {
	constructor() {
		super(data);
	}

	getById(id: string): TemplateResponseModel | undefined {
		const item = this.data.find((item) => item.id === id);
		return item ? createTemplate(item) : undefined;
	}

	getItemById(id: string): TemplateItemResponseModel | null {
		const item = this.data.find((item) => item.id === id);
		return item ? createTemplateItem(item) : null;
	}

	getScaffold(): TemplateScaffoldResponseModel {
		return {
			content:
				'@using Umbraco.Cms.Web.Common.PublishedModels;\r\n@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n@{\r\n\tLayout = null;\r\n}',
		};
	}

	getItems(ids: string[]): TemplateItemResponseModel[] {
		const items = ids.map((id) => this.getItemById(id)).filter((item) => item !== null) as TemplateItemResponseModel[];
		return items;
	}

	getTemplateQuerySettings = () => templateQuerySettings;

	create(templateData: CreateTemplateRequestModel) {
		const template = {
			$type: '',
			id: UmbId.new(),
			...templateData,
		};
		this.data.push(template);
		return template;
	}

	update(template: TemplateResponseModel) {
		this.updateData(template);
		return template;
	}

	getTreeRoot(): PagedEntityTreeItemResponseModel {
		const items = this.data.filter((item) => item.parentId === null);
		const treeItems = items.map((item) => createEntityTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItemChildren(id: string): PagedEntityTreeItemResponseModel {
		const items = this.data.filter((item) => item.parentId === id);
		const treeItems = items.map((item) => createEntityTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItem(ids: Array<string>): Array<EntityTreeItemResponseModel> {
		const items = this.data.filter((item) => ids.includes(item.id ?? ''));
		return items.map((item) => createEntityTreeItem(item));
	}
}

export const umbTemplateData = new UmbTemplateData();
