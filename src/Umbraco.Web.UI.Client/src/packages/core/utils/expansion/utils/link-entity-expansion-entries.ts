import type { UmbEntityExpansionEntryModel, UmbEntityExpansionModel } from '../types.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export const linkEntityExpansionEntries = (data: Array<UmbEntityModel>): UmbEntityExpansionModel => {
	return data.map((item, index) => {
		const result: UmbEntityExpansionEntryModel = {
			entityType: item.entityType,
			unique: item.unique,
		};

		const next = data[index + 1];

		if (next) {
			result.target = {
				entityType: next.entityType,
				unique: next.unique,
			};
		}

		return result;
	});
};
