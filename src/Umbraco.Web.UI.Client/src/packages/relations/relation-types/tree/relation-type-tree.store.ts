import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';

/**
 * @export
 * @class UmbRelationTypeTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for Relation Type Items
 */
export class UmbRelationTypeTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbRelationTypeTreeStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbRelationTypeTreeStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_RELATION_TYPE_TREE_STORE_CONTEXT.toString());
	}
}

export default UmbRelationTypeTreeStore;

export const UMB_RELATION_TYPE_TREE_STORE_CONTEXT = new UmbContextToken<UmbRelationTypeTreeStore>(
	'UmbRelationTypeTreeStore',
);
