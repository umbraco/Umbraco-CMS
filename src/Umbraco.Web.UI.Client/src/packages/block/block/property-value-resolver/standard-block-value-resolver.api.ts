import type { UmbBlockExposeModel, UmbBlockValueType } from '../types.js';
import { UmbBlockValueResolver, type UmbBlockValuesCallback } from './block-value-resolver.api.js';
import type { UmbElementValueModel } from '@umbraco-cms/backoffice/content';

export class UmbStandardBlockValueResolver extends UmbBlockValueResolver<UmbBlockValueType> {
	async processValues(property: UmbElementValueModel<UmbBlockValueType>, valuesCallback: UmbBlockValuesCallback) {
		if (property.value) {
			return {
				...property,
				value: await this._processValueBlockData(property.value, valuesCallback),
			};
		}
		return property;
	}

	async processVariants(
		property: UmbElementValueModel<UmbBlockValueType>,
		variantsCallback: (values: Array<UmbBlockExposeModel>) => Promise<Array<UmbBlockExposeModel> | undefined>,
	) {
		if (property.value) {
			return {
				...property,
				value: await this._processVariantBlockData(property.value, variantsCallback),
			};
		}
		return property;
	}
}
