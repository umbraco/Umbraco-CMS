import type { UmbUserGroupDetailModel } from '../../types.js';
import { UmbUserGroupServerDataSource } from './user-group-detail.server.data-source.js';
import { UMB_USER_GROUP_DETAIL_STORE_CONTEXT } from './user-group-detail.store.token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbUserGroupDetailRepository extends UmbDetailRepositoryBase<UmbUserGroupDetailModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbUserGroupServerDataSource, UMB_USER_GROUP_DETAIL_STORE_CONTEXT);
	}

	override async create(model: UmbUserGroupDetailModel) {
		return super.create(model, null);
	}
}
export default UmbUserGroupDetailRepository;
