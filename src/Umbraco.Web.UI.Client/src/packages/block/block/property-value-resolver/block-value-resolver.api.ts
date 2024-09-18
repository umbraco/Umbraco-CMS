import type { UmbBlockValueType } from '../types.js';
import type { UmbContentValueModel } from '@umbraco-cms/backoffice/content';
import type { UmbPropertyValueResolver } from '@umbraco-cms/backoffice/property';

export class UmbBlockValueResolver
	implements UmbPropertyValueResolver<UmbContentValueModel<UmbBlockValueType>, UmbContentValueModel>
{
	process(
		property: UmbContentValueModel<UmbBlockValueType>,
		propertyValueMethod: (entry: UmbContentValueModel) => UmbContentValueModel,
	) {
		const value = property.value
			? {
					...property.value,
					contentData: property.value.contentData?.map((entry) => ({
						...entry,
						values: entry.values.map((value) => propertyValueMethod(value)),
					})),
					settingsData: property.value.settingsData.map((entry) => ({
						...entry,
						values: entry.values.map((value) => propertyValueMethod(value)),
					})),
				}
			: undefined;
		return {
			...property,
			value,
		};
	}

	destroy(): void {}
}
