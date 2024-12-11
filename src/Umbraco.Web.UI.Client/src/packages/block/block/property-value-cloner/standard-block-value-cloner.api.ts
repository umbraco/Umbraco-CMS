import type { UmbBlockDataValueModel, UmbBlockExposeModel, UmbBlockValueType } from '../types.js';
import { UmbBlockValueCloner } from './block-value-cloner.api.js';
import type { UmbElementValueModel } from '@umbraco-cms/backoffice/content';

export class UmbStandardBlockValueCloner extends UmbBlockValueCloner<UmbBlockValueType> {
	async processValues(
		property: UmbElementValueModel<UmbBlockValueType>,
		valuesCallback: (values: Array<UmbBlockDataValueModel>) => Promise<Array<UmbBlockDataValueModel> | undefined>,
	) {
		if (property.value) {
			return {
				...property,
				value: await this._processValueBlockData(property.value, valuesCallback),
			};
		}
		return property;
	}
}
