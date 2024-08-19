import type { UmbRelationTypeDetailModel } from '../../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
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
	 * @param {UmbControllerHost} host
	 * @memberof UmbRelationTypeDetailStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_RELATION_TYPE_DETAIL_STORE_CONTEXT.toString());
	}
}

export default UmbRelationTypeDetailStore;

export const UMB_RELATION_TYPE_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbRelationTypeDetailStore>(
	'UmbRelationTypeDetailStore',
);
