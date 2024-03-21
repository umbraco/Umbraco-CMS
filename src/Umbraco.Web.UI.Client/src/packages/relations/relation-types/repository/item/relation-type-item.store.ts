import type { UmbRelationTypeItemModel } from './types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbRelationTypeItemStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Relation Type items
 */

export class UmbRelationTypeItemStore extends UmbItemStoreBase<UmbRelationTypeItemModel> {
	/**
	 * Creates an instance of UmbRelationTypeItemStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbRelationTypeItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_RELATION_TYPE_ITEM_STORE_CONTEXT.toString());
	}
}

export const UMB_RELATION_TYPE_ITEM_STORE_CONTEXT = new UmbContextToken<UmbRelationTypeItemStore>(
	'UmbRelationTypeItemStore',
);
