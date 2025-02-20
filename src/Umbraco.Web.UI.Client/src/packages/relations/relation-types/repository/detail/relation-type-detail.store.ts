import type { UmbRelationTypeDetailModel } from '../../types.js';
import { UMB_RELATION_TYPE_DETAIL_STORE_CONTEXT } from './relation-type-detail.store.context-token.js';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * @class UmbRelationTypeDetailStore
 * @augments {UmbStoreBase}
 * @description - Data Store for RelationType Details
 */
export class UmbRelationTypeDetailStore extends UmbDetailStoreBase<UmbRelationTypeDetailModel> {
	/**
	 * Creates an instance of UmbRelationTypeDetailStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbRelationTypeDetailStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_RELATION_TYPE_DETAIL_STORE_CONTEXT.toString());
	}
}

export default UmbRelationTypeDetailStore;
