import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbEntityTreeStore } from '@umbraco-cms/backoffice/tree';

/**
 * @export
 * @class UmbDictionaryTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for Dictionary Items
 */
export class UmbDictionaryTreeStore extends UmbEntityTreeStore {
	/**
	 * Creates an instance of UmbDictionaryTreeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDictionaryTreeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_DICTIONARY_TREE_STORE_CONTEXT.toString());
	}
}

export const UMB_DICTIONARY_TREE_STORE_CONTEXT = new UmbContextToken<UmbDictionaryTreeStore>('UmbDictionaryTreeStore');
