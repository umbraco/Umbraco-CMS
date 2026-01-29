import type { UmbMockTemplateModel } from '../../types/mock-data-set.types.js';
import type {
	TemplateQuerySettingsResponseModel,
	TemplateQueryResultResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { TemplateQueryPropertyTypeModel, OperatorModel } from '@umbraco-cms/backoffice/external/backend-api';

export const data: Array<UmbMockTemplateModel> = [
	{
		"id": "34c10c5b-6fc0-41de-88f0-c436a06dd21c",
		"parent": null,
		"name": "Home",
		"hasChildren": false,
		"alias": "home",
		"flags": [],
		"content": "@using Umbraco.Cms.Web.Common.PublishedModels;\n@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\n@{\n\tLayout = null;\n}"
	},
	{
		"id": "592d4920-7a78-4c5f-917e-40eae48b03bb",
		"parent": {
			"id": "7761ac21-1dad-43f3-835f-12635d23b0e9"
		},
		"name": "Test Page",
		"hasChildren": false,
		"alias": "testPage",
		"flags": [],
		"content": "@using Umbraco.Cms.Web.Common.PublishedModels;\n@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\n@{\n\tLayout = \"testPage\";\n}",
		"masterTemplate": {
			"id": "7761ac21-1dad-43f3-835f-12635d23b0e9"
		}
	},
	{
		"id": "a83e40ed-a267-4d78-9db0-d309f0e814ff",
		"parent": {
			"id": "7761ac21-1dad-43f3-835f-12635d23b0e9"
		},
		"name": "Rich Text Editor Tiptap",
		"hasChildren": false,
		"alias": "richTextEditorTiptap",
		"flags": [],
		"content": "@using Umbraco.Cms.Web.Common.PublishedModels;\n@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\n@{\n\tLayout = \"richTextEditorTiptap\";\n}",
		"masterTemplate": {
			"id": "7761ac21-1dad-43f3-835f-12635d23b0e9"
		}
	},
	{
		"id": "7761ac21-1dad-43f3-835f-12635d23b0e9",
		"parent": null,
		"name": "Layout",
		"hasChildren": true,
		"alias": "layout",
		"flags": [],
		"content": "@using Umbraco.Cms.Web.Common.PublishedModels;\n@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\n@{\n\tLayout = null;\n}"
	},
	{
		"id": "2829dfed-8894-4cdc-825b-cfb5c53f5771",
		"parent": {
			"id": "7761ac21-1dad-43f3-835f-12635d23b0e9"
		},
		"name": "Rich Text Editor TinyMCE",
		"hasChildren": false,
		"alias": "richTextEditorTinyMce",
		"flags": [],
		"content": "@using Umbraco.Cms.Web.Common.PublishedModels;\n@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\n@{\n\tLayout = \"richTextEditorTinyMce\";\n}",
		"masterTemplate": {
			"id": "7761ac21-1dad-43f3-835f-12635d23b0e9"
		}
	},
	{
		"id": "40dfb1d7-108b-4a08-a989-b6e709e592e4",
		"parent": null,
		"name": "Parent",
		"hasChildren": true,
		"alias": "parent",
		"flags": [],
		"content": "@using Umbraco.Cms.Web.Common.PublishedModels;\n@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\n@{\n\tLayout = null;\n}"
	},
	{
		"id": "814cdb58-b984-4e2b-9969-2040c3b244a9",
		"parent": {
			"id": "40dfb1d7-108b-4a08-a989-b6e709e592e4"
		},
		"name": "Child",
		"hasChildren": false,
		"alias": "child",
		"flags": [],
		"content": "@using Umbraco.Cms.Web.Common.PublishedModels;\n@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\n@{\n\tLayout = \"child\";\n}",
		"masterTemplate": {
			"id": "40dfb1d7-108b-4a08-a989-b6e709e592e4"
		}
	},
	{
		"id": "b05279f3-d62c-4a6f-b2ff-fd920b1269a3",
		"parent": null,
		"name": "Test Master",
		"hasChildren": true,
		"alias": "testMaster",
		"flags": [],
		"content": "@using Umbraco.Cms.Web.Common.PublishedModels;\n@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\n@{\n\tLayout = null;\n}"
	},
	{
		"id": "33b9a6ae-9b06-4a11-9ae7-6d48136181ea",
		"parent": {
			"id": "b05279f3-d62c-4a6f-b2ff-fd920b1269a3"
		},
		"name": "Test Child",
		"hasChildren": false,
		"alias": "testChild",
		"flags": [],
		"content": "@using Umbraco.Cms.Web.Common.PublishedModels;\n@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\n@{\n\tLayout = \"testChild\";\n}",
		"masterTemplate": {
			"id": "b05279f3-d62c-4a6f-b2ff-fd920b1269a3"
		}
	},
	{
		"id": "5af84edd-3eca-4070-a52b-087f17ce9681",
		"parent": {
			"id": "b05279f3-d62c-4a6f-b2ff-fd920b1269a3"
		},
		"name": "Test Child 2",
		"hasChildren": false,
		"alias": "testChild2",
		"flags": [],
		"content": "@using Umbraco.Cms.Web.Common.PublishedModels;\n@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\n@{\n\tLayout = \"testChild2\";\n}",
		"masterTemplate": {
			"id": "b05279f3-d62c-4a6f-b2ff-fd920b1269a3"
		}
	}
];

export const templateQueryResult: TemplateQueryResultResponseModel = {
	queryExpression: '',
	sampleResults: [],
	resultCount: 0,
	executionTime: 0,
};

export const templateQuerySettings: TemplateQuerySettingsResponseModel = {
	documentTypeAliases: [],
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
			operator: OperatorModel.GREATER_THAN,
			applicableTypes: [TemplateQueryPropertyTypeModel.INTEGER, TemplateQueryPropertyTypeModel.DATE_TIME],
		},
		{
			operator: OperatorModel.CONTAINS,
			applicableTypes: [TemplateQueryPropertyTypeModel.STRING],
		},
	],
};
