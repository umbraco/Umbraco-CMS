import { dataSet } from '../data/sets/index.js';
import type {
	TemplateQueryResultResponseModel,
	TemplateQuerySettingsResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

const defaultQuerySettings: TemplateQuerySettingsResponseModel = {
	documentTypeAliases: [],
	properties: [],
	operators: [],
};

const defaultQueryResult: TemplateQueryResultResponseModel = {
	queryExpression: '',
	sampleResults: [],
	resultCount: 0,
	executionTime: 0,
};

export class UmbMockTemplateQueryManager {
	constructor() {}

	getQuerySettings = () => dataSet.templateQuerySettings ?? defaultQuerySettings;

	getQueryResult = () => dataSet.templateQueryResult ?? defaultQueryResult;
}
