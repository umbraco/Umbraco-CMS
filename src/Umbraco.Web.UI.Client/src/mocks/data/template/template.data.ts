import type {
	TemplateResponseModel,
	TemplateItemResponseModel,
	TemplateQuerySettingsResponseModel,
	TemplateQueryResultResponseModel,
	NamedEntityTreeItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { TemplateQueryPropertyTypeModel, OperatorModel } from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockTemplateModel = TemplateResponseModel & NamedEntityTreeItemResponseModel & TemplateItemResponseModel;

export const data: Array<UmbMockTemplateModel> = [
	{
		id: '2bf464b6-3aca-4388-b043-4eb439cc2643',
		parent: null,
		name: 'Doc 1',
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
		hasChildren: true,
		alias: 'Test',
		content:
			'@using Umbraco.Cms.Web.Common.PublishedModels;\n@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<ContentModels.Test>\r\n@using ContentModels = Umbraco.Cms.Web.Common.PublishedModels;\r\n@{\r\n\tLayout = null;\r\n}',
	},
	{
		id: '9a84c0b3-03b4-4dd4-84ac-706740ac0f72',
		parent: { id: '9a84c0b3-03b4-4dd4-84ac-706740ac0f71' },
		masterTemplate: { id: '9a84c0b3-03b4-4dd4-84ac-706740ac0f71' },
		name: 'Child',
		hasChildren: false,
		alias: 'Test',
		content:
			'@using Umbraco.Cms.Web.Common.PublishedModels;\n@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<ContentModels.Test>\r\n@using ContentModels = Umbraco.Cms.Web.Common.PublishedModels;\r\n@{\r\n\tLayout = "Test.cshtml";\r\n}',
	},
	{
		id: '9a84c0b3-03b4-4dd4-84ac-706740acwerer0f72',
		parent: { id: '9a84c0b3-03b4-4dd4-84ac-706740ac0f71' },
		name: 'Has Master Template',
		masterTemplate: { id: '9a84c0b3-03b4-4dd4-84ac-706740ac0f71' },
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
	documentTypeAliases: ['docTypeWithTemplate', 'propertyTypeWithTemplate', 'somethingElse'],
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
