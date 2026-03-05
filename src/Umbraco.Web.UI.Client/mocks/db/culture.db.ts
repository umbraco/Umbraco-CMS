import { culturesMock as data } from '../data/culture.data.js';
import type { PagedCultureReponseModel } from '@umbraco-cms/backoffice/external/backend-api';

class UmbCulturesData {
	get(): PagedCultureReponseModel {
		return { total: data.length, items: data };
	}
}

export const umbCulturesData = new UmbCulturesData();
