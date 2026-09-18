import { UmbMediaCollectionSortPreferenceServerDataSource } from './sort-preference.server.data-source.js';
import type { UmbMediaCollectionSortPreferenceModel, UmbMediaCollectionSortPreferenceStoreModel } from './types.js';
import { UmbRepositoryBase, type UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

let preferencePromise: Promise<UmbRepositoryResponse<UmbMediaCollectionSortPreferenceStoreModel>> | undefined;
let cachedPreference: UmbMediaCollectionSortPreferenceStoreModel | undefined;

export class UmbMediaCollectionSortPreferenceRepository extends UmbRepositoryBase {
	readonly #serverDataSource = new UmbMediaCollectionSortPreferenceServerDataSource(this);

	async requestPreference(): Promise<UmbRepositoryResponse<UmbMediaCollectionSortPreferenceStoreModel>> {
		if (cachedPreference) {
			return { data: cachedPreference };
		}

		preferencePromise ??= this.#serverDataSource.getPreference();
		const response = await preferencePromise;

		if (response.error) {
			preferencePromise = undefined;
			return response;
		}

		if (response.data) {
			cachedPreference = response.data;
		}

		return response;
	}

	async savePreference(
		preference: UmbMediaCollectionSortPreferenceModel,
	): Promise<UmbRepositoryResponse<UmbMediaCollectionSortPreferenceStoreModel>> {
		const current = cachedPreference ?? (await this.requestPreference()).data;
		const payload: UmbMediaCollectionSortPreferenceStoreModel = {
			key: current?.key,
			orderBy: preference.orderBy,
			orderDirection: preference.orderDirection,
		};

		const { error } = await this.#serverDataSource.savePreference(payload);
		if (error) {
			return { error };
		}

		if (!payload.key) {
			const { data, error: reloadError } = await this.#reloadPreference();
			if (reloadError) {
				return { error: reloadError };
			}

			cachedPreference = data;
			return { data };
		}

		cachedPreference = payload;
		return { data: payload };
	}

	async #reloadPreference() {
		preferencePromise = undefined;
		cachedPreference = undefined;
		return this.requestPreference();
	}
}

/**
 * Test-only.
 * @internal
 */
export function resetUmbMediaCollectionSortPreferenceCache(): void {
	preferencePromise = undefined;
	cachedPreference = undefined;
}
