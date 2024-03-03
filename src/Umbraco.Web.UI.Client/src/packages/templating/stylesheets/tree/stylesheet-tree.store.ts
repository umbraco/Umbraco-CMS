import type { UmbStylesheetDetailModel } from '../types.js';
import { UMB_STYLESHEET_DETAIL_STORE_CONTEXT } from '../repository/index.js';
import type { UmbStylesheetTreeItemModel } from './types.js';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbStoreConnector } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbStylesheetTreeStore
 * @extends {UmbUniqueTreeStore}
 * @description - Tree Data Store for Stylesheets
 */
export class UmbStylesheetTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbStylesheetTreeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbStylesheetTreeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_STYLESHEET_TREE_STORE_CONTEXT.toString());

		new UmbStoreConnector<UmbStylesheetTreeItemModel, UmbStylesheetDetailModel>(host, {
			store: this,
			connectToStoreAlias: UMB_STYLESHEET_DETAIL_STORE_CONTEXT,
			updateStoreItemMapper: (item) => this.#updateTreeItemMapper(item),
		});
	}

	#updateTreeItemMapper = (item: UmbStylesheetDetailModel) => {
		return {
			name: item.name,
		};
	};
}

export default UmbStylesheetTreeStore;

export const UMB_STYLESHEET_TREE_STORE_CONTEXT = new UmbContextToken<UmbStylesheetTreeStore>('UmbStylesheetTreeStore');
