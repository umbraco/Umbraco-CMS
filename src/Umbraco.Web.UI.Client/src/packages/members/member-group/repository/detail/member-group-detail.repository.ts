import type { UmbMemberGroupDetailModel } from '../../types.js';
import { UmbMemberGroupServerDataSource } from './member-group-detail.server.data-source.js';
import type { UmbMemberGroupDetailStore } from './member-group-detail.store.js';
import { UMB_MEMBER_GROUP_DETAIL_STORE_CONTEXT } from './member-group-detail.store.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
export class UmbMemberGroupDetailRepository extends UmbDetailRepositoryBase<UmbMemberGroupDetailModel> {
	#init: Promise<unknown>;
	#detailStore?: UmbMemberGroupDetailStore;

	constructor(host: UmbControllerHost) {
		super(host, UmbMemberGroupServerDataSource, UMB_MEMBER_GROUP_DETAIL_STORE_CONTEXT);

		this.#init = Promise.all([
			this.consumeContext(UMB_MEMBER_GROUP_DETAIL_STORE_CONTEXT, (instance) => {
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
