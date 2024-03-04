import { UMB_PARTIAL_VIEW_DETAIL_STORE_CONTEXT } from '../repository/partial-view-detail.store.js';
import type { UmbPartialViewDetailModel } from '../types.js';
import type { UmbPartialViewTreeItemModel } from './types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';
import { UmbStoreConnector } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbPartialViewTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for PartialView
 */
export class UmbPartialViewTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbPartialViewTreeStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbPartialViewTreeStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_PARTIAL_VIEW_TREE_STORE_CONTEXT.toString());

		new UmbStoreConnector<UmbPartialViewTreeItemModel, UmbPartialViewDetailModel>(host, {
			store: this,
			connectToStoreAlias: UMB_PARTIAL_VIEW_DETAIL_STORE_CONTEXT,
			updateStoreItemMapper: (item) => this.#updateTreeItemMapper(item),
		});
	}

	#updateTreeItemMapper = (item: UmbPartialViewDetailModel) => {
		return {
			name: item.name,
		};
	};
}

export default UmbPartialViewTreeStore;

export const UMB_PARTIAL_VIEW_TREE_STORE_CONTEXT = new UmbContextToken<UmbPartialViewTreeStore>(
	'UmbPartialViewTreeStore',
);
