import type { UmbBlockDataValueModel, UmbBlockExposeModel, UmbBlockValueDataPropertiesBaseType } from '../types.js';
import type { UmbElementValueModel } from '@umbraco-cms/backoffice/content';
import type { UmbPropertyValueResolver } from '@umbraco-cms/backoffice/property';

export type UmbBlockValuesCallback = (
	values: Array<UmbBlockDataValueModel>,
	identifier?: string,
) => Promise<Array<UmbBlockDataValueModel> | undefined>;

export abstract class UmbBlockValueResolver<ValueType> implements UmbPropertyValueResolver<
	UmbElementValueModel<ValueType>,
	UmbBlockDataValueModel,
	UmbBlockExposeModel
> {
	abstract processValues(
		property: UmbElementValueModel<ValueType>,
		valuesCallback: UmbBlockValuesCallback,
	): Promise<UmbElementValueModel<ValueType>>;

	protected async _processValueBlockData<ValueType extends UmbBlockValueDataPropertiesBaseType>(
		value: ValueType,
		valuesCallback: UmbBlockValuesCallback,
	) {
		const contentData = await Promise.all(
			(value.contentData ?? []).map(async (entry) => ({
				...entry,
				// We do not know for sure if the same key could be used for both content and settings data, so we prefix the key with the type to ensure uniqueness.
				values: (await valuesCallback(entry.values, `contentData:${entry.key}`)) ?? [],
			})),
		);
		const settingsData = await Promise.all(
			(value.settingsData ?? []).map(async (entry) => ({
				...entry,
				// We do not know for sure if the same key could be used for both content and settings data, so we prefix the key with the type to ensure uniqueness.
				values: (await valuesCallback(entry.values, `settingsData:${entry.key}`)) ?? [],
			})),
		);
		return { ...value, contentData, settingsData };
	}

	abstract processVariants(
		property: UmbElementValueModel<ValueType>,
		variantsCallback: (values: Array<UmbBlockExposeModel>) => Promise<Array<UmbBlockExposeModel> | undefined>,
	): Promise<UmbElementValueModel<ValueType>>;

	protected async _processVariantBlockData<ValueType extends UmbBlockValueDataPropertiesBaseType>(
		value: ValueType,
		variantsCallback: (values: Array<UmbBlockExposeModel>) => Promise<Array<UmbBlockExposeModel> | undefined>,
	) {
		const expose = (await variantsCallback(value.expose ?? [])) ?? [];
		return { ...value, expose };
	}

	compareVariants(a: UmbBlockExposeModel, b: UmbBlockExposeModel) {
		return a.contentKey === b.contentKey && a.culture === b.culture && a.segment === b.segment;
	}

	destroy(): void {}
}
