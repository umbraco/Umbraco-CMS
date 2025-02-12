import type { UmbBlockDataValueModel, UmbBlockExposeModel, UmbBlockValueDataPropertiesBaseType } from '../types.js';
import type { UmbElementValueModel } from '@umbraco-cms/backoffice/content';
import type { UmbPropertyValueResolver } from '@umbraco-cms/backoffice/property';

export abstract class UmbBlockValueResolver<ValueType>
	implements UmbPropertyValueResolver<UmbElementValueModel<ValueType>, UmbBlockDataValueModel, UmbBlockExposeModel>
{
	abstract processValues(
		property: UmbElementValueModel<ValueType>,
		valuesCallback: (values: Array<UmbBlockDataValueModel>) => Promise<Array<UmbBlockDataValueModel> | undefined>,
	): Promise<UmbElementValueModel<ValueType>>;

	protected async _processValueBlockData<ValueType extends UmbBlockValueDataPropertiesBaseType>(
		value: ValueType,
		valuesCallback: (values: Array<UmbBlockDataValueModel>) => Promise<Array<UmbBlockDataValueModel> | undefined>,
	) {
		const contentData = await Promise.all(
			value.contentData?.map(async (entry) => ({
				...entry,
				values: (await valuesCallback(entry.values)) ?? [],
			})),
		);
		const settingsData = await Promise.all(
			value.settingsData?.map(async (entry) => ({
				...entry,
				values: (await valuesCallback(entry.values)) ?? [],
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
		const expose = (await variantsCallback(value.expose)) ?? [];
		return { ...value, expose };
	}

	compareVariants(a: UmbBlockExposeModel, b: UmbBlockExposeModel) {
		return a.contentKey === b.contentKey && a.culture === b.culture && a.segment === b.segment;
	}

	destroy(): void {}
}
