import type { UmbMemberDetailModel } from '../../types.js';
import { UmbMemberServerDataSource } from './member-detail.server.data-source.js';
import { UMB_MEMBER_DETAIL_STORE_CONTEXT } from './member-detail.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';

/**
 * A repository for Member details
 * @class UmbMemberDetailRepository
 * @augments {UmbDetailRepositoryBase<UmbMemberDetailModel>}
 */
export class UmbMemberDetailRepository extends UmbDetailRepositoryBase<UmbMemberDetailModel> {
	/**
	 * Creates an instance of UmbMemberDetailRepository.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMemberDetailRepository
	 */
	constructor(host: UmbControllerHost) {
		super(host, UmbMemberServerDataSource, UMB_MEMBER_DETAIL_STORE_CONTEXT);
	}

	override async create(model: UmbMemberDetailModel) {
		return super.create(model, null);
	}
}

export default UmbMemberDetailRepository;
