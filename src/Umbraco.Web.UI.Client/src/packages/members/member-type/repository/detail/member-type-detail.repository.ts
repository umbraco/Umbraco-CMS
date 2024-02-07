import type { UmbMemberTypeDetailModel } from '../../types.js';
import { UmbMemberTypeServerDataSource } from './member-type-detail.server.data-source.js';
import type { UmbMemberTypeDetailStore } from './member-type-detail.store.js';
import { UMB_MEMBER_TYPE_DETAIL_STORE_CONTEXT } from './member-type-detail.store.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
export class UmbMemberTypeDetailRepository extends UmbDetailRepositoryBase<UmbMemberTypeDetailModel> {
	#init: Promise<unknown>;
	#detailStore?: UmbMemberTypeDetailStore;

	constructor(host: UmbControllerHost) {
		super(host, UmbMemberTypeServerDataSource, UMB_MEMBER_TYPE_DETAIL_STORE_CONTEXT);

		this.#init = Promise.all([
			this.consumeContext(UMB_MEMBER_TYPE_DETAIL_STORE_CONTEXT, (instance) => {
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
