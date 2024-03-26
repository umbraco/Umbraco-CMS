import type { UmbDictionaryDetailModel } from '../types.js';
import { UMB_DICTIONARY_DETAIL_STORE_CONTEXT } from '../repository/detail/dictionary-detail.store.js';
import type { UmbDictionaryTreeItemModel } from './types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbStoreConnector } from '@umbraco-cms/backoffice/store';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';

/**
 * @export
 * @class UmbDictionaryTreeStore
 * @extends {UmbUniqueTreeStore}
 * @description - Tree Data Store for Dictionary Items
 */
export class UmbDictionaryTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbDictionaryTreeStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDictionaryTreeStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_DICTIONARY_TREE_STORE_CONTEXT.toString());

		new UmbStoreConnector<UmbDictionaryTreeItemModel, UmbDictionaryDetailModel>(host, {
			store: this,
			connectToStoreAlias: UMB_DICTIONARY_DETAIL_STORE_CONTEXT,
			updateStoreItemMapper: (item) => this.#updateTreeItemMapper(item),
		});
	}

	#updateTreeItemMapper = (model: UmbDictionaryDetailModel) => {
		return {
			name: model.name,
		};
	};
}

export const UMB_DICTIONARY_TREE_STORE_CONTEXT = new UmbContextToken<UmbDictionaryTreeStore>('UmbDictionaryTreeStore');
