import type { UmbDocumentDetailModel } from '../types.js';
import { UMB_DOCUMENT_DETAIL_STORE_CONTEXT } from '../repository/detail/index.js';
import type { UmbDocumentTreeItemModel } from './types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbStoreConnector } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbDocumentTreeStore
 * @extends {UmbUniqueTreeStore}
 * @description - Tree Data Store for Document Items
 */
export class UmbDocumentTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbDocumentTreeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDocumentTreeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_DOCUMENT_TREE_STORE_CONTEXT.toString());

		new UmbStoreConnector<UmbDocumentTreeItemModel, UmbDocumentDetailModel>(host, {
			store: this,
			connectToStoreAlias: UMB_DOCUMENT_DETAIL_STORE_CONTEXT,
			updateStoreItemMapper: (item) => this.#updateTreeItemMapper(item),
		});
	}

	#updateTreeItemMapper = (item: UmbDocumentDetailModel) => {
		return {
			variants: item.variants.map((variant) => {
				return {
					name: variant.name,
					culture: variant.culture,
					state: variant.state,
				};
			}),
		};
	};
}

export const UMB_DOCUMENT_TREE_STORE_CONTEXT = new UmbContextToken<UmbDocumentTreeStore>('UmbDocumentTreeStore');
