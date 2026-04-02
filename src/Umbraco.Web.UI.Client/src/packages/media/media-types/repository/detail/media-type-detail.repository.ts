import type { UmbMediaTypeDetailModel } from '../../types.js';
import { UmbMediaTypeDetailServerDataSource } from './media-type-detail.server.data-source.js';
import { UMB_MEDIA_TYPE_DETAIL_STORE_CONTEXT } from './media-type-detail.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbDetailRepositoryBase,
	type UmbRepositoryResponseWithAsObservable,
} from '@umbraco-cms/backoffice/repository';
import type { UmbDetailStore } from '@umbraco-cms/backoffice/store';

export class UmbMediaTypeDetailRepository extends UmbDetailRepositoryBase<
	UmbMediaTypeDetailModel,
	UmbMediaTypeDetailServerDataSource
> {
	#init: Promise<unknown>;
	#detailStore?: UmbDetailStore<UmbMediaTypeDetailModel>;

	constructor(host: UmbControllerHost) {
		super(host, UmbMediaTypeDetailServerDataSource, UMB_MEDIA_TYPE_DETAIL_STORE_CONTEXT);

		this.#init = this.consumeContext(UMB_MEDIA_TYPE_DETAIL_STORE_CONTEXT, (instance) => {
			this.#detailStore = instance;
		})
			.asPromise({ preventTimeout: true })
			.catch(() => undefined);
	}

	/**
	 * Requests multiple media type details by their unique IDs
	 * @param {Array<string>} uniques - The unique IDs of the media types to fetch
	 * @returns {Promise<UmbRepositoryResponseWithAsObservable<Array<UmbMediaTypeDetailModel>>>}
	 * @memberof UmbMediaTypeDetailRepository
	 */
	async requestByUniques(
		uniques: Array<string>,
	): Promise<UmbRepositoryResponseWithAsObservable<Array<UmbMediaTypeDetailModel> | undefined>> {
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

export default UmbMediaTypeDetailRepository;
