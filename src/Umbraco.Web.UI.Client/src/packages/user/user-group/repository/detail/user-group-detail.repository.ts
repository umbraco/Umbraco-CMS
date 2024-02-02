import type { UmbUserGroupDetailModel } from '../../types.js';
import { UmbUserGroupServerDataSource } from './user-group-detail.server.data-source.js';
import type { UmbUserGroupDetailStore } from './user-group-detail.store.js';
import { UMB_USER_GROUP_DETAIL_STORE_CONTEXT } from './user-group-detail.store.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
export class UmbUserGroupDetailRepository extends UmbDetailRepositoryBase<UmbUserGroupDetailModel> {
	#init: Promise<unknown>;
	#detailStore?: UmbUserGroupDetailStore;

	constructor(host: UmbControllerHost) {
		super(host, UmbUserGroupServerDataSource, UMB_USER_GROUP_DETAIL_STORE_CONTEXT);

		this.#init = Promise.all([
			this.consumeContext(UMB_USER_GROUP_DETAIL_STORE_CONTEXT, (instance) => {
				this.#detailStore = instance;
			}).asPromise(),
		]);
	}

	async byPropertyEditorUiAlias(propertyEditorUiAlias: string) {
		if (!propertyEditorUiAlias) throw new Error('propertyEditorUiAlias is missing');
		await this.#init;
		return this.#detailStore!.withPropertyEditorUiAlias(propertyEditorUiAlias);
	}
}
