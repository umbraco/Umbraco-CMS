import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UmbEntityTreeStore } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbRelationTypeTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for relation-types
 */
// TODO: consider if tree store could be turned into a general EntityTreeStore class?
export class UmbRelationTypeTreeStore extends UmbEntityTreeStore {
	/**
	 * Creates an instance of UmbRelationTypeTreeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbRelationTypeTreeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_RELATION_TYPE_TREE_STORE_CONTEXT_TOKEN.toString());
	}
}

export const UMB_RELATION_TYPE_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbRelationTypeTreeStore>(
	'UmbRelationTypeTreeStore'
);
