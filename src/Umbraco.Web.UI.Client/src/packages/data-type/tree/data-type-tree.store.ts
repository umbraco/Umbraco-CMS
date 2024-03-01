import { UMB_DATA_TYPE_DETAIL_STORE_CONTEXT } from '../repository/index.js';
import type { UmbDataTypeDetailModel } from '../types.js';
import type { UmbDataTypeTreeItemModel } from './types.js';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';
import { UmbStoreConnector } from '@umbraco-cms/backoffice/store';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

/**
 * @export
 * @class UmbDataTypeTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for Data Types
 */
export class UmbDataTypeTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbDataTypeTreeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDataTypeTreeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_DATA_TYPE_TREE_STORE_CONTEXT.toString());

		new UmbStoreConnector<UmbDataTypeTreeItemModel, UmbDataTypeDetailModel>(host, {
			store: this,
			connectToStoreAlias: UMB_DATA_TYPE_DETAIL_STORE_CONTEXT,
			updateStoreItemMapper: (item) => this.#updateTreeItemMapper(item),
		});
	}

	#updateTreeItemMapper = (item: UmbDataTypeDetailModel) => {
		return {
			name: item.name,
		};
	};
}

export const UMB_DATA_TYPE_TREE_STORE_CONTEXT = new UmbContextToken<UmbDataTypeTreeStore>('UmbDataTypeTreeStore');
