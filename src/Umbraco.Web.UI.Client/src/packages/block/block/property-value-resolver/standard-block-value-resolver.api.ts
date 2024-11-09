import type { UmbBlockDataValueModel, UmbBlockExposeModel, UmbBlockValueType } from '../types.js';
import { UmbBlockValueResolver } from './block-value-resolver.api.js';
import type { UmbContentValueModel } from '@umbraco-cms/backoffice/content';

export class UmbStandardBlockValueResolver extends UmbBlockValueResolver<UmbBlockValueType> {
	async processValues(
		property: UmbContentValueModel<UmbBlockValueType>,
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

	async processVariants(
		property: UmbContentValueModel<UmbBlockValueType>,
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
