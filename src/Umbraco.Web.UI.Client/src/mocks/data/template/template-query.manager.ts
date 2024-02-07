import type { UmbEntityMockDbBase } from '../utils/entity/entity-base.js';
import { templateQueryResult, templateQuerySettings, type UmbMockTemplateModel } from './template.data.js';

export class UmbMockTemplateQueryManager {
	#db: UmbEntityMockDbBase<UmbMockTemplateModel>;

	constructor(db: UmbEntityMockDbBase<UmbMockTemplateModel>) {
		this.#db = db;
	}

	getQuerySettings = () => templateQuerySettings;

	getQueryResult = () => templateQueryResult;
}
