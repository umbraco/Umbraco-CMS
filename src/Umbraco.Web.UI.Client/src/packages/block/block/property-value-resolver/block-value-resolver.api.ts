import type { UmbBlockDataValueModel, UmbBlockExposeModel, UmbBlockValueDataPropertiesBaseType } from '../types.js';
import type { UmbContentValueModel } from '@umbraco-cms/backoffice/content';
import type { UmbPropertyValueResolver } from '@umbraco-cms/backoffice/property';

export abstract class UmbBlockValueResolver<ValueType>
	implements UmbPropertyValueResolver<UmbContentValueModel<ValueType>, UmbBlockDataValueModel, UmbBlockExposeModel>
{
	abstract processValues(
		property: UmbContentValueModel<ValueType>,
		valuesCallback: (values: Array<UmbBlockDataValueModel>) => Promise<Array<UmbBlockDataValueModel> | undefined>,
	): Promise<UmbContentValueModel<ValueType>>;

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
		property: UmbContentValueModel<ValueType>,
		variantsCallback: (values: Array<UmbBlockExposeModel>) => Promise<Array<UmbBlockExposeModel> | undefined>,
	): Promise<UmbContentValueModel<ValueType>>;

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

	/*
	async ensureVariants(
		property: UmbContentValueModel<UmbBlockValueType>,
		args: UmbPropertyValueResolverEnsureVariantArgs,
	) {
		if (property.value && args.selectedVariants) {
			const currentExposes = property.value.expose ?? [];
			const contentKeys = property.value.contentData.map((x) => x.key);

			const newExposes = contentKeys.flatMap((contentKey) =>
				args.selectedVariants.map((v) => ({
					contentKey: contentKey,
					culture: v.culture,
					segment: v.segment,
				})),
			) as Array<UmbBlockExposeModel>;

			// make exposes unique:
			const expose = [
				...currentExposes,
				...newExposes.filter(
					(n) =>
						!currentExposes.some(
							(p) => p.contentKey === n.contentKey && p.culture === n.culture && p.segment === n.segment,
						),
				),
			];

			return {
				...property,
				value: {
					...property.value,
					expose,
				},
			};
		}
		return property;
	}
		*/

	destroy(): void {}
}
