import { UmbContextToken } from '@umbraco-cms/context-api';
import { UmbTreeStoreBase } from '@umbraco-cms/store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

/**
 * @export
 * @class UmbDictionaryTreeStore
 * @extends {UmbTreeStoreBase}
 * @description - Tree Data Store for Dictionary
 */
export class UmbDictionaryTreeStore extends UmbTreeStoreBase {
	/**
	 * Creates an instance of UmbDictionaryTreeStore.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbDictionaryTreeStore
	 */
	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_DICTIONARY_TREE_STORE_CONTEXT_TOKEN.toString());
	}
}

export const UMB_DICTIONARY_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDictionaryTreeStore>(
	'UmbDictionaryTreeStore'
);
