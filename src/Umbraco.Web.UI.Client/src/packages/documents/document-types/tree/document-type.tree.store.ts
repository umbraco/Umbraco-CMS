import type { UmbDocumentTypeDetailModel } from '../types.js';
import { UMB_DOCUMENT_TYPE_DETAIL_STORE_CONTEXT } from '../repository/index.js';
import type { UmbDocumentTypeTreeItemModel } from './types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';
import { UmbStoreConnector } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbDocumentTypeTreeStore
 * @extends {UmbUniqueTreeStore}
 * @description - Tree Data Store for Document Types
 */
export class UmbDocumentTypeTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbDocumentTypeTreeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDocumentTypeTreeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_DOCUMENT_TYPE_TREE_STORE_CONTEXT.toString());

		new UmbStoreConnector<UmbDocumentTypeTreeItemModel, UmbDocumentTypeDetailModel>(host, {
			store: this,
			connectToStoreAlias: UMB_DOCUMENT_TYPE_DETAIL_STORE_CONTEXT,
			updateStoreItemMapper: (item) => this.#updateTreeItemMapper(item),
		});
	}

	#updateTreeItemMapper = (item: UmbDocumentTypeDetailModel) => {
		return {
			name: item.name,
			icon: item.icon,
		};
	};
}

export const UMB_DOCUMENT_TYPE_TREE_STORE_CONTEXT = new UmbContextToken<UmbDocumentTypeTreeStore>(
	'UmbDocumentTypeTreeStore',
);
