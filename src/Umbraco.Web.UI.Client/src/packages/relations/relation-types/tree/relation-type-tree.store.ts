import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbEntityTreeStore } from '@umbraco-cms/backoffice/tree';

/**
 * @export
 * @class UmbRelationTypeTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for Relation Type Items
 */
export class UmbRelationTypeTreeStore extends UmbEntityTreeStore {
	/**
	 * Creates an instance of UmbRelationTypeTreeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbRelationTypeTreeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_RELATION_TYPE_TREE_STORE_CONTEXT.toString());
	}
}

export const UMB_RELATION_TYPE_TREE_STORE_CONTEXT = new UmbContextToken<UmbRelationTypeTreeStore>(
	'UmbRelationTypeTreeStore',
);
