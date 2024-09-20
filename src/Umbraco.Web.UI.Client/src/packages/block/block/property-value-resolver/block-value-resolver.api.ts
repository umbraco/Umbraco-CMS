import type { UmbBlockDataValueModel, UmbBlockValueType } from '../types.js';
import type { UmbContentValueModel } from '@umbraco-cms/backoffice/content';
import type { UmbPropertyValueResolver } from '@umbraco-cms/backoffice/property';

export class UmbBlockValueResolver
	implements UmbPropertyValueResolver<UmbContentValueModel<UmbBlockValueType>, UmbBlockDataValueModel>
{
	async processValues(
		property: UmbContentValueModel<UmbBlockValueType>,
		valuesCallback: (values: Array<UmbBlockDataValueModel>) => Promise<Array<UmbBlockDataValueModel> | undefined>,
	) {
		if (property.value) {
			const contentData = await Promise.all(
				property.value.contentData?.map(async (entry) => ({
					...entry,
					values: (await valuesCallback(entry.values)) ?? [],
				})),
			);
			const settingsData = await Promise.all(
				property.value.settingsData?.map(async (entry) => ({
					...entry,
					values: (await valuesCallback(entry.values)) ?? [],
				})),
			);

			return {
				...property,
				value: {
					...property.value,
					contentData,
					settingsData,
				},
			};
		}
		return property;
	}

	destroy(): void {}
}
