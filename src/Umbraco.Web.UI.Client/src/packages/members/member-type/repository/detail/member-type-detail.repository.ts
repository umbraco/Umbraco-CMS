import type { UmbMemberTypeDetailModel } from '../../types.js';
import { UmbMemberTypeServerDataSource } from './member-type-detail.server.data-source.js';
import { UMB_MEMBER_TYPE_DETAIL_STORE_CONTEXT } from './member-type-detail.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';

/**
 * A repository for the Member Type detail
 * @class UmbMemberTypeDetailRepository
 * @augments {UmbDetailRepositoryBase<UmbMemberTypeDetailModel>}
 */
export class UmbMemberTypeDetailRepository extends UmbDetailRepositoryBase<UmbMemberTypeDetailModel> {
	/**
	 * Creates an instance of UmbMemberTypeDetailRepository.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMemberTypeDetailRepository
	 */
	constructor(host: UmbControllerHost) {
		super(host, UmbMemberTypeServerDataSource, UMB_MEMBER_TYPE_DETAIL_STORE_CONTEXT);
	}
}

export default UmbMemberTypeDetailRepository;
