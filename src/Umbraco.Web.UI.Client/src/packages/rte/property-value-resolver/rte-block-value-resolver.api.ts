import type { UmbPropertyEditorUiValueType } from '../types.js';
import {
	UmbBlockValueResolver,
	type UmbBlockDataValueModel,
	type UmbBlockExposeModel,
} from '@umbraco-cms/backoffice/block';
import type { UmbElementValueModel } from '@umbraco-cms/backoffice/content';

export class UmbRteBlockValueResolver extends UmbBlockValueResolver<UmbPropertyEditorUiValueType> {
	async processValues(
		property: UmbElementValueModel<UmbPropertyEditorUiValueType>,
		valuesCallback: (values: Array<UmbBlockDataValueModel>) => Promise<Array<UmbBlockDataValueModel> | undefined>,
	) {
		if (property.value) {
			return {
				...property,
				value: {
					...property.value,
					blocks: await this._processValueBlockData(property.value.blocks, valuesCallback),
				},
			};
		}
		return property;
	}

	async processVariants(
		property: UmbElementValueModel<UmbPropertyEditorUiValueType>,
		variantsCallback: (values: Array<UmbBlockExposeModel>) => Promise<Array<UmbBlockExposeModel> | undefined>,
	) {
		if (property.value) {
			return {
				...property,
				value: {
					...property.value,
					blocks: await this._processVariantBlockData(property.value.blocks, variantsCallback),
				},
			};
		}
		return property;
	}
}
