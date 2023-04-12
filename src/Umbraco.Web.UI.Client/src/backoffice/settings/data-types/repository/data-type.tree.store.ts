import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UmbEntityTreeStore } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbDataTypeTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for Data-Types
 */
// TODO: consider if tree store could be turned into a general EntityTreeStore class?
export class UmbDataTypeTreeStore extends UmbEntityTreeStore {
	/**
	 * Creates an instance of UmbDataTypeTreeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDataTypeTreeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_DATA_TYPE_TREE_STORE_CONTEXT_TOKEN.toString());
	}
}

export const UMB_DATA_TYPE_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDataTypeTreeStore>('UmbDataTypeTreeStore');
