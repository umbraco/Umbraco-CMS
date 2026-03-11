import type { UmbDataTypeDetailModel } from '../../types.js';
import { UmbDataTypeServerDataSource } from './server-data-source/data-type-detail.server.data-source.js';
import type { UmbDataTypeDetailStore } from './data-type-detail.store.js';
import { UMB_DATA_TYPE_DETAIL_STORE_CONTEXT } from './data-type-detail.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbDetailRepositoryBase,
	type UmbRepositoryResponseWithAsObservable,
} from '@umbraco-cms/backoffice/repository';

export class UmbDataTypeDetailRepository extends UmbDetailRepositoryBase<
	UmbDataTypeDetailModel,
	UmbDataTypeServerDataSource
> {
	#init: Promise<unknown>;
	#detailStore?: UmbDataTypeDetailStore;

	constructor(host: UmbControllerHost) {
		super(host, UmbDataTypeServerDataSource, UMB_DATA_TYPE_DETAIL_STORE_CONTEXT);

		this.#init = this.consumeContext(UMB_DATA_TYPE_DETAIL_STORE_CONTEXT, (instance) => {
			this.#detailStore = instance;
		})
			.asPromise({ preventTimeout: true })
			.catch(() => {
				// If the context is not available, we can assume that the store is not available.
				this.#detailStore = undefined;
			});
	}

	/**
	 * Requests multiple data type details by their unique IDs
	 * @param {Array<string>} uniques - The unique IDs of the data types to fetch
	 * @returns {Promise<UmbRepositoryResponseWithAsObservable<Array<UmbDataTypeDetailModel>>>}
	 * @memberof UmbDataTypeDetailRepository
	 */
	async requestByUniques(
		uniques: Array<string>,
	): Promise<UmbRepositoryResponseWithAsObservable<Array<UmbDataTypeDetailModel> | undefined>> {
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

	async byPropertyEditorUiAlias(propertyEditorUiAlias: string) {
		if (!propertyEditorUiAlias) throw new Error('propertyEditorUiAlias is missing');
		await this.#init;
		return this.#detailStore!.withPropertyEditorUiAlias(propertyEditorUiAlias);
	}
}

export { UmbDataTypeDetailRepository as api };
