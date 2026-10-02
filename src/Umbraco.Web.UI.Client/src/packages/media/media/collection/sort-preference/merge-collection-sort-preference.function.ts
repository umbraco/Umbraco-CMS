import type { UmbCollectionConfiguration } from '@umbraco-cms/backoffice/collection';
import type { UmbMediaCollectionSortPreferenceModel } from './types.js';

export function mergeMediaCollectionSortPreference(
	config: UmbCollectionConfiguration,
	preference?: UmbMediaCollectionSortPreferenceModel | null,
): UmbCollectionConfiguration {
	if (!preference?.orderBy || !preference.orderDirection) {
		return config;
	}

	return {
		...config,
		orderBy: preference.orderBy,
		orderDirection: preference.orderDirection,
	};
}
