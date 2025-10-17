import type { UmbTemplateDetailModel, UmbTemplateForDocumentTypeDetailModel } from '../../types.js';
import { UmbTemplateServerDataSource } from './template-detail.server.data-source.js';
import { UMB_TEMPLATE_DETAIL_STORE_CONTEXT } from './template-detail.store.context-token.js';
import type UmbTemplateDetailStore from './template-detail.store.js';
import { UmbDetailRepositoryBase, type UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbTemplateDetailRepository extends UmbDetailRepositoryBase<UmbTemplateDetailModel> {
	#init: Promise<unknown>;
	#detailStore?: UmbTemplateDetailStore;

	constructor(host: UmbControllerHost) {
		super(host, UmbTemplateServerDataSource, UMB_TEMPLATE_DETAIL_STORE_CONTEXT);

		this.#init = this.consumeContext(UMB_TEMPLATE_DETAIL_STORE_CONTEXT, (instance) => {
			this.#detailStore = instance;
		})
			.asPromise({ preventTimeout: true })
			.catch(() => {
				// If the context is not available, we can assume that the store is not available.
				this.#detailStore = undefined;
			});
	}

	/**
	 * Returns a promise with an observable of the detail for the given unique
	 * @param {UmbTemplateForDocumentTypeDetailModel} model
	 * @returns {*}
	 * @memberof UmbTemplateDetailRepository
	 */
	async createForDocumentType(
		model: UmbTemplateForDocumentTypeDetailModel,
	): Promise<UmbRepositoryResponse<UmbTemplateDetailModel>> {
		if (!model) throw new Error('Data is missing');

		const { data: createdData, error } = await (
			this.detailDataSource as UmbTemplateServerDataSource
		).createForDocumentType(model);

		if (createdData) {
			await this.#init;
			this.#detailStore?.append(createdData);
		}

		return { data: createdData, error };
	}
}

export default UmbTemplateDetailRepository;
