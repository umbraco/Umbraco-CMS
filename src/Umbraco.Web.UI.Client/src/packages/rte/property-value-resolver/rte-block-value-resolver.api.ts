import type { UmbPropertyEditorRteValueType } from '../types.js';
import {
	UmbBlockValueResolver,
	type UmbBlockExposeModel,
	type UmbBlockValuesCallback,
} from '@umbraco-cms/backoffice/block';
import type { UmbEntryValueModel } from '@umbraco-cms/backoffice/content';

export class UmbRteBlockValueResolver extends UmbBlockValueResolver<UmbPropertyEditorRteValueType> {
	async processValues(
		property: UmbEntryValueModel<UmbPropertyEditorRteValueType>,
		valuesCallback: UmbBlockValuesCallback,
	) {
		if (property.value) {
			return {
				...property,
				value: {
					...property.value,
					blocks: property.value?.blocks
						? await this._processValueBlockData(property.value.blocks, valuesCallback)
						: undefined,
				},
			};
		}
		return property;
	}

	async processVariants(
		property: UmbEntryValueModel<UmbPropertyEditorRteValueType>,
		variantsCallback: (values: Array<UmbBlockExposeModel>) => Promise<Array<UmbBlockExposeModel> | undefined>,
	) {
		if (property.value) {
			return {
				...property,
				value: {
					...property.value,
					blocks: property.value?.blocks
						? await this._processVariantBlockData(property.value.blocks, variantsCallback)
						: undefined,
				},
			};
		}
		return property;
	}
}
