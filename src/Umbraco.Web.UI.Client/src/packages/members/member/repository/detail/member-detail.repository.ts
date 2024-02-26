import type { UmbMemberDetailModel } from '../../types.js';
import { UmbMemberServerDataSource } from './member-detail.server.data-source.js';
import { UMB_MEMBER_DETAIL_STORE_CONTEXT } from './member-detail.store.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';

/**
 * A repository for Member details
 * @export
 * @class UmbMemberDetailRepository
 * @extends {UmbDetailRepositoryBase<UmbMemberDetailModel>}
 */
export class UmbMemberDetailRepository extends UmbDetailRepositoryBase<UmbMemberDetailModel> {
	/**
	 * Creates an instance of UmbMemberDetailRepository.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMemberDetailRepository
	 */
	constructor(host: UmbControllerHost) {
		super(host, UmbMemberServerDataSource, UMB_MEMBER_DETAIL_STORE_CONTEXT);
	}
}

export default UmbMemberDetailRepository;
