import type { UmbMockTemplateModel } from '../../mock-data-set.types.js';
import type {
	TemplateQuerySettingsResponseModel,
	TemplateQueryResultResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { TemplateQueryPropertyTypeModel, OperatorModel } from '@umbraco-cms/backoffice/external/backend-api';

export const data: Array<UmbMockTemplateModel> = [];

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
