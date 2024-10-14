import { templateQueryResult, templateQuerySettings } from './template.data.js';

export class UmbMockTemplateQueryManager {
	constructor() {}

	getQuerySettings = () => templateQuerySettings;

	getQueryResult = () => templateQueryResult;
}
