import { templateQueryResult, templateQuerySettings } from '../data/template/template.data.js';

export class UmbMockTemplateQueryManager {
	constructor() {}

	getQuerySettings = () => templateQuerySettings;

	getQueryResult = () => templateQueryResult;
}
