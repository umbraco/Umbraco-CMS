import { templateQueryResult, templateQuerySettings, type UmbMockTemplateModel } from './template.data.js';

export class UmbMockTemplateQueryManager {
	constructor() {}

	getQuerySettings = () => templateQuerySettings;

	getQueryResult = () => templateQueryResult;
}
