import type { UmbElementDetailModel } from '../../types.js';
import { UmbElementServerDataSource } from './element-detail.server.data-source.js';
import type { UmbElementDetailStore } from './element-detail.store.js';
import { UMB_ELEMENT_DETAIL_STORE_CONTEXT } from './element-detail.store.context-token.js';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbRepositoryResponseWithAsObservable } from '@umbraco-cms/backoffice/repository';

export class UmbElementDetailRepository extends UmbDetailRepositoryBase<
	UmbElementDetailModel,
	UmbElementServerDataSource
> {
	#init: Promise<unknown>;
	#detailStore?: UmbElementDetailStore;

	constructor(host: UmbControllerHost) {
		super(host, UmbElementServerDataSource, UMB_ELEMENT_DETAIL_STORE_CONTEXT);

		this.#init = this.consumeContext(UMB_ELEMENT_DETAIL_STORE_CONTEXT, (instance) => {
			this.#detailStore = instance;
		})
			.asPromise({ preventTimeout: true })
			.catch(() => {
				// If the context is not available, we can assume that the store is not available.
				this.#detailStore = undefined;
			});
	}

	/**
	 * Requests multiple element details by their unique IDs
	 * @param {Array<string>} uniques - The unique IDs of the elements to fetch
	 * @returns {Promise<UmbRepositoryResponseWithAsObservable<Array<UmbElementDetailModel> | undefined>>}
	 * @memberof UmbElementDetailRepository
	 */
	async requestByUniques(
		uniques: Array<string>,
	): Promise<UmbRepositoryResponseWithAsObservable<Array<UmbElementDetailModel> | undefined>> {
		if (!uniques || uniques.length === 0) {
			return { data: [] };
		}

		await this.#init;

		const { data, error } = await this.detailDataSource.readMany(uniques);

		if (data) {
			data.forEach((item) => this.#detailStore?.append(item));
		}

		return {
			data,
			error,
			asObservable: () => this.#detailStore?.byUniques(uniques),
		};
	}
}

export { UmbElementDetailRepository as api };
