import type { UmbDataTypeDetailModel } from '../../types.js';
import { UmbDataTypeServerDataSource } from './data-type-detail.server.data-source.js';
import type { UmbDataTypeDetailStore } from './data-type-detail.store.js';
import { UMB_DATA_TYPE_DETAIL_STORE_CONTEXT } from './data-type-detail.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
export class UmbDataTypeDetailRepository extends UmbDetailRepositoryBase<UmbDataTypeDetailModel> {
	#init: Promise<unknown>;
	#detailStore?: UmbDataTypeDetailStore;

	constructor(host: UmbControllerHost) {
		super(host, UmbDataTypeServerDataSource, UMB_DATA_TYPE_DETAIL_STORE_CONTEXT);

		this.#init = this.consumeContext(UMB_DATA_TYPE_DETAIL_STORE_CONTEXT, (instance) => {
			this.#detailStore = instance;
		}).asPromise({ preventTimeout: true });
	}

	async byPropertyEditorUiAlias(propertyEditorUiAlias: string) {
		if (!propertyEditorUiAlias) throw new Error('propertyEditorUiAlias is missing');
		await this.#init;
		return this.#detailStore!.withPropertyEditorUiAlias(propertyEditorUiAlias);
	}
}

export { UmbDataTypeDetailRepository as api };
