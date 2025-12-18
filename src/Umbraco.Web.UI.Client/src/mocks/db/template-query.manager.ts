import { templateQueryResult, templateQuerySettings } from '../data/template.data.js';

export class UmbMockTemplateQueryManager {
	constructor() {}

	getQuerySettings = () => templateQuerySettings;

	getQueryResult = () => templateQueryResult;
}
