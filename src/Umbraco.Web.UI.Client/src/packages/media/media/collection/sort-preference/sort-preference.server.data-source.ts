import {
	UMB_MEDIA_COLLECTION_SORT_PREFERENCE_GROUP,
	UMB_MEDIA_COLLECTION_SORT_PREFERENCE_IDENTIFIER,
} from './constants.js';
import type { UmbMediaCollectionSortPreferenceStoreModel } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UserDataService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class UmbMediaCollectionSortPreferenceServerDataSource extends UmbControllerBase {
	async getPreference(): Promise<UmbDataSourceResponse<UmbMediaCollectionSortPreferenceStoreModel>> {
		const { data, error } = await tryExecute(
			this,
			UserDataService.getUserData({
				query: {
					groups: [UMB_MEDIA_COLLECTION_SORT_PREFERENCE_GROUP],
					identifiers: [UMB_MEDIA_COLLECTION_SORT_PREFERENCE_IDENTIFIER],
					take: 1,
				},
			}),
		);

		if (error) {
			return { error };
		}

		const item = data?.items?.[0];
		if (!item?.value) {
			return { data: undefined };
		}

		try {
			const parsed = JSON.parse(item.value) as UmbMediaCollectionSortPreferenceStoreModel;
			if (!parsed.orderBy || !parsed.orderDirection) {
				return { data: undefined };
			}

			return {
				data: {
					key: item.key,
					orderBy: parsed.orderBy,
					orderDirection: parsed.orderDirection,
				},
			};
		} catch {
			return { data: undefined };
		}
	}

	async savePreference(
		preference: UmbMediaCollectionSortPreferenceStoreModel,
	): Promise<UmbDataSourceResponse<void>> {
		const value = JSON.stringify({
			orderBy: preference.orderBy,
			orderDirection: preference.orderDirection,
		});

		if (preference.key) {
			const { error } = await tryExecute(
				this,
				UserDataService.putUserData({
					body: {
						key: preference.key,
						group: UMB_MEDIA_COLLECTION_SORT_PREFERENCE_GROUP,
						identifier: UMB_MEDIA_COLLECTION_SORT_PREFERENCE_IDENTIFIER,
						value,
					},
				}),
			);

			return error ? { error } : {};
		}

		const { data, error } = await tryExecute(
			this,
			UserDataService.postUserData({
				body: {
					group: UMB_MEDIA_COLLECTION_SORT_PREFERENCE_GROUP,
					identifier: UMB_MEDIA_COLLECTION_SORT_PREFERENCE_IDENTIFIER,
					value,
				},
			}),
		);

		if (error) {
			return { error };
		}

		if (data) {
			return {
				data: undefined,
			};
		}

		return {};
	}
}
