import type { UmbUserDetailModel } from '../../types.js';
import type { UmbUserDetailDataSource } from './types.js';
import { UmbUserServerDataSource } from './user-detail.server.data-source.js';
import { UMB_USER_DETAIL_STORE_CONTEXT } from './user-detail.store.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbUserDetailRepository extends UmbDetailRepositoryBase<UmbUserDetailModel, UmbUserDetailDataSource> {
	constructor(host: UmbControllerHost) {
		super(host, UmbUserServerDataSource, UMB_USER_DETAIL_STORE_CONTEXT);
	}

	/**
	 * Creates a new User detail
	 * @param {UmbUserDetailModel} model
	 * @return {*}
	 * @memberof UmbUserDetailRepository
	 */
	async create(model: UmbUserDetailModel) {
		return super.create(model, null);
	}

	/**
	 * Requests the detail for the given unique
	 * @return {*}
	 * @memberof UmbUserDetailRepository
	 */
	requestCalculateStartNodes(unique: string) {
		return this.detailDataSource.calculateStartNodes(unique);
	}
}

export default UmbUserDetailRepository;
