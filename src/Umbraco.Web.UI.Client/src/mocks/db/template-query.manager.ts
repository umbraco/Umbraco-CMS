import { dataSet } from '../data/sets/index.js';

const templateQueryResult = dataSet.templateQueryResult;
const templateQuerySettings = dataSet.templateQuerySettings;

export class UmbMockTemplateQueryManager {
	constructor() {}

	getQuerySettings = () => templateQuerySettings;

	getQueryResult = () => templateQueryResult;
}
