import { dataSet } from '../data/sets/index.js';

export class UmbMockTemplateQueryManager {
	constructor() {}

	getQuerySettings = () => dataSet.templateQuerySettings;

	getQueryResult = () => dataSet.templateQueryResult;
}
