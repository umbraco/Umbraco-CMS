import type { UmbMemberGroupDetailModel } from '../../types.js';
import { UmbMemberGroupServerDataSource } from './member-group-detail.server.data-source.js';
import { UMB_MEMBER_GROUP_DETAIL_STORE_CONTEXT } from './member-group-detail.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';

/**
 * A repository for fetching member group details.
 * @class UmbMemberGroupDetailRepository
 * @augments {UmbDetailRepositoryBase<UmbMemberGroupDetailModel>}
 */
export class UmbMemberGroupDetailRepository extends UmbDetailRepositoryBase<UmbMemberGroupDetailModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbMemberGroupServerDataSource, UMB_MEMBER_GROUP_DETAIL_STORE_CONTEXT);
	}

	override async create(model: UmbMemberGroupDetailModel) {
		return super.create(model, null);
	}
}

export default UmbMemberGroupDetailRepository;
