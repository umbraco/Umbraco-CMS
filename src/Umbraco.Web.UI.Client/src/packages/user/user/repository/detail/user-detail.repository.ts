import type { UmbUserDetailModel } from '../../types.js';
import { UmbUserServerDataSource } from './user-detail.server.data-source.js';
import { UMB_USER_DETAIL_STORE_CONTEXT } from './user-detail.store.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbUserDetailRepository extends UmbDetailRepositoryBase<UmbUserDetailModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbUserServerDataSource, UMB_USER_DETAIL_STORE_CONTEXT);
	}

	async create(model: UmbUserDetailModel) {
		return super.create(model, null);
	}
}
