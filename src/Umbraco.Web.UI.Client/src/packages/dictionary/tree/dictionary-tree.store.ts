import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';

/**
 * @class UmbDictionaryTreeStore
 * @augments {UmbUniqueTreeStore}
 * @description - Tree Data Store for Dictionary Items
 */
export class UmbDictionaryTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbDictionaryTreeStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDictionaryTreeStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_DICTIONARY_TREE_STORE_CONTEXT.toString());
	}
}

export { UmbDictionaryTreeStore as api };

export const UMB_DICTIONARY_TREE_STORE_CONTEXT = new UmbContextToken<UmbDictionaryTreeStore>('UmbDictionaryTreeStore');
