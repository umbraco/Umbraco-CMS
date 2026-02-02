import type { UmbDocumentTypeDetailModel } from '../../types.js';
import { UMB_DOCUMENT_TYPE_DETAIL_STORE_CONTEXT } from './document-type-detail.store.context-token.js';
import { UmbDocumentTypeDetailServerDataSource } from './server-data-source/document-type-detail.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase, type UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbDetailStore } from '@umbraco-cms/backoffice/store';

export class UmbDocumentTypeDetailRepository extends UmbDetailRepositoryBase<
	UmbDocumentTypeDetailModel,
	UmbDocumentTypeDetailServerDataSource
> {
	#init: Promise<unknown>;
	#detailStore?: UmbDetailStore<UmbDocumentTypeDetailModel>;

	constructor(host: UmbControllerHost) {
		super(host, UmbDocumentTypeDetailServerDataSource, UMB_DOCUMENT_TYPE_DETAIL_STORE_CONTEXT);

		this.#init = this.consumeContext(UMB_DOCUMENT_TYPE_DETAIL_STORE_CONTEXT, (instance) => {
			this.#detailStore = instance;
		})
			.asPromise({ preventTimeout: true })
			.catch(() => undefined);
	}

	/**
	 * Requests multiple document type details by their unique IDs
	 * @param {Array<string>} uniques - The unique IDs of the document types to fetch
	 * @returns {Promise<UmbRepositoryResponse<Array<UmbDocumentTypeDetailModel>>>}
	 * @memberof UmbDocumentTypeDetailRepository
	 */
	async requestByUniques(
		uniques: Array<string>,
	): Promise<UmbRepositoryResponse<Array<UmbDocumentTypeDetailModel> | undefined>> {
		if (!uniques || uniques.length === 0) {
			return { data: [] };
		}

		await this.#init;

		const { data, error } = await this.detailDataSource.readMany(uniques);

		if (data) {
			data.forEach((item) => this.#detailStore?.append(item));
		}

		return { data, error };
	}
}

export { UmbDocumentTypeDetailRepository as api };
