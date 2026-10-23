import type { UmbRelationTypeItemModel } from './types.js';
import { UMB_RELATION_TYPE_ITEM_STORE_CONTEXT } from './relation-type-item.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @class UmbRelationTypeItemStore
 * @augments {UmbItemStoreBase}
 * @description - Data Store for Relation Type items
 */

export class UmbRelationTypeItemStore extends UmbItemStoreBase<UmbRelationTypeItemModel> {
	/**
	 * Creates an instance of UmbRelationTypeItemStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbRelationTypeItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_RELATION_TYPE_ITEM_STORE_CONTEXT);
	}
}

export default UmbRelationTypeItemStore;
