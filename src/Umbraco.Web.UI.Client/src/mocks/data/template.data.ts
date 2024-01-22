import { UmbEntityData } from './entity.data.js';
import { createEntityTreeItem } from './utils.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import {
	EntityTreeItemResponseModel,
	TemplateResponseModel,
	TemplateScaffoldResponseModel,
	CreateTemplateRequestModel,
	TemplateItemResponseModel,
	TemplateQuerySettingsResponseModel,
	TemplateQueryPropertyTypeModel,
	OperatorModel,
	TemplateQueryResultResponseModel,
	PagedNamedEntityTreeItemResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

type UmbMockTemplateModel = TemplateResponseModel & EntityTreeItemResponseModel;

const createTemplate = (dbItem: UmbMockTemplateModel): TemplateResponseModel => {
	return {
		id: dbItem.id,
		name: dbItem.name,
		alias: dbItem.alias,
		content: dbItem.content,
		masterTemplateId: dbItem.masterTemplateId,
	};
};

const createTemplateItem = (dbItem: UmbMockTemplateModel): TemplateItemResponseModel => ({
	name: dbItem.name,
	id: dbItem.id,
	alias: dbItem.alias,
});

export const data: Array<UmbMockTemplateModel> = [
	{
		id: '2bf464b6-3aca-4388-b043-4eb439cc2643',
		parent: null,
		name: 'Doc 1',
		type: 'templa }te',
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
		id: '9a84c0b3-03b4-4dd4-84ac-706740ac0f71',
		parent: null,
		name: 'Test',
		type: 'templat }e',
		hasChildren: true,
		alias: 'Test',
		content:
			'@using Umbraco.Cms.Web.Common.PublishedModels;\n@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<ContentModels.Test>\r\n@using ContentModels = Umbraco.Cms.Web.Common.PublishedModels;\r\n@{\r\n\tLayout = null;\r\n}',
	},
	{
		id: '9a84c0b3-03b4-4dd4-84ac-706740ac0f72',
		parent: { id: '9a84c0b3-03b4-4dd4-84ac-706740ac0f71' },
		masterTemplateId: '9a84c0b3-03b4-4dd4-84ac-706740ac0f71',
		name: 'Child',
		type: 'template',
		hasChildren: false,
		alias: 'Test',
		content:
			'@using Umbraco.Cms.Web.Common.PublishedModels;\n@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<ContentModels.Test>\r\n@using ContentModels = Umbraco.Cms.Web.Common.PublishedModels;\r\n@{\r\n\tLayout = "Test.cshtml";\r\n}',
	},
	{
		id: '9a84c0b3-03b4-4dd4-84ac-706740acwerer0f72',
		parent: { id: '9a84c0b3-03b4-4dd4-84ac-706740ac0f71' },
		name: 'Has Master Template',
		masterTemplateId: '9a84c0b3-03b4-4dd4-84ac-706740ac0f71',
		type: 'template',
		hasChildren: false,
		alias: 'hasMasterTemplate',
		content:
			'@using Umbraco.Cms.Web.Common.PublishedModels;\n@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<ContentModels.Test>\r\n@using ContentModels = Umbraco.Cms.Web.Common.PublishedModels;\r\n@{\r\n\tLayout = "Test.cshtml";\r\n}',
	},
];

export const createTemplateScaffold = (masterTemplateAlias: string) => {
	return `@using Umbraco.Cms.Web.Common.PublishedModels;
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage
@{
	Layout = ${masterTemplateAlias};
}`;
};

//prettier-ignore
const templateQueryExpressions = [
`Umbraco.ContentAtRoot().FirstOrDefault()
.ChildrenOfType("docTypeWithTemplate")
.Where(x => x.IsVisible())`,
`Umbraco.Content(Guid.Parse("0b3498dc-255a-4d62-aa4f-ef7bff333544"))
.ChildrenOfType("docTypeWithTemplate")
.Where(x => x.IsVisible())`,
`Umbraco.ContentAtRoot().FirstOrDefault()
.ChildrenOfType("docTypeWithTemplate")
.Where(x => (x.Id != -6))
.Where(x => x.IsVisible())
.OrderBy(x => x.UpdateDate)`,
];

const randomIndex = () => Math.floor(Math.random() * 3);

export const templateQueryResult: TemplateQueryResultResponseModel = {
	queryExpression: templateQueryExpressions[randomIndex()],
	sampleResults: [],
	resultCount: 0,
	executionTime: 0,
};

export const templateQuerySettings: TemplateQuerySettingsResponseModel = {
	contentTypeAliases: ['docTypeWithTemplate', 'propertyTypeWithTemplate', 'somethingElse'],
	properties: [
		{
			alias: 'Id',
			type: TemplateQueryPropertyTypeModel.INTEGER,
		},
		{
			alias: 'Name',
			type: TemplateQueryPropertyTypeModel.STRING,
		},
		{
			alias: 'CreateDate',
			type: TemplateQueryPropertyTypeModel.DATE_TIME,
		},
		{
			alias: 'UpdateDate',
			type: TemplateQueryPropertyTypeModel.DATE_TIME,
		},
	],
	operators: [
		{
			operator: OperatorModel.EQUALS,
			applicableTypes: [TemplateQueryPropertyTypeModel.INTEGER, TemplateQueryPropertyTypeModel.STRING],
		},
		{
			operator: OperatorModel.NOT_EQUALS,
			applicableTypes: [TemplateQueryPropertyTypeModel.INTEGER, TemplateQueryPropertyTypeModel.STRING],
		},
		{
			operator: OperatorModel.LESS_THAN,
			applicableTypes: [TemplateQueryPropertyTypeModel.INTEGER, TemplateQueryPropertyTypeModel.DATE_TIME],
		},
		{
			operator: OperatorModel.GREATER_THAN_EQUAL_TO,
			applicableTypes: [TemplateQueryPropertyTypeModel.INTEGER, TemplateQueryPropertyTypeModel.DATE_TIME],
		},
		{
			operator: OperatorModel.GREATER_THAN,
			applicableTypes: [TemplateQueryPropertyTypeModel.INTEGER, TemplateQueryPropertyTypeModel.DATE_TIME],
		},
		{
			operator: OperatorModel.GREATER_THAN_EQUAL_TO,
			applicableTypes: [TemplateQueryPropertyTypeModel.INTEGER, TemplateQueryPropertyTypeModel.DATE_TIME],
		},
		{
			operator: OperatorModel.CONTAINS,
			applicableTypes: [TemplateQueryPropertyTypeModel.STRING],
		},
		{
			operator: OperatorModel.NOT_CONTAINS,
			applicableTypes: [TemplateQueryPropertyTypeModel.STRING],
		},
	],
};

// Temp mocked database
class UmbTemplateData extends UmbEntityData<UmbMockTemplateModel> {
	constructor() {
		super(data);
	}

	// eslint-disable-next-line @typescript-eslint/ban-ts-comment
	// @ts-ignore
	getById(id: string): TemplateResponseModel | undefined {
		const item = this.data.find((item) => item.id === id);
		return item ? createTemplate(item) : undefined;
	}

	getItemById(id: string): TemplateItemResponseModel | null {
		const item = this.data.find((item) => item.id === id);
		return item ? createTemplateItem(item) : null;
	}

	getScaffold(masterTemplateId: string | null = null): TemplateScaffoldResponseModel {
		const masterTemplate = this.data.find((item) => item.id === masterTemplateId);

		return {
			content: createTemplateScaffold(masterTemplate?.alias ?? 'null'),
		};
	}

	getItems(ids: string[]): TemplateItemResponseModel[] {
		const items = ids.map((id) => this.getItemById(id)).filter((item) => item !== null) as TemplateItemResponseModel[];
		return items;
	}

	getTemplateQuerySettings = () => templateQuerySettings;

	getTemplateQueryResult = () => templateQueryResult;

	create(templateData: CreateTemplateRequestModel) {
		const template = {
			id: UmbId.new(),
			type: 'template',
			hasChildren: false,
			isContainer: false,
			...templateData,
		};
		this.data.push(template);
		return template;
	}

	update(template: TemplateResponseModel) {
		this.updateData(template);
		return template;
	}

	getTreeRoot(): PagedNamedEntityTreeItemResponseModel {
		const items = this.data.filter((item) => item.parent?.id === null);
		const treeItems = items.map((item) => createEntityTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItemChildren(id: string): PagedNamedEntityTreeItemResponseModel {
		const items = this.data.filter((item) => item.parent?.id === id);
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
