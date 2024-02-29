import type { UmbMediaTypeDetailModel } from '../types.js';
import { UMB_MEDIA_TYPE_DETAIL_STORE_CONTEXT } from '../repository/index.js';
import type { UmbMediaTypeTreeItemModel } from './types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbStoreConnector } from '@umbraco-cms/backoffice/store';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';

/**
 * @export
 * @class UmbMediaTypeTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for Media Types
 */
export class UmbMediaTypeTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbMediaTypeTreeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbMediaTypeTreeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEDIA_TYPE_TREE_STORE_CONTEXT.toString());

		new UmbStoreConnector<UmbMediaTypeTreeItemModel, UmbMediaTypeDetailModel>(host, {
			store: this,
			connectToStoreAlias: UMB_MEDIA_TYPE_DETAIL_STORE_CONTEXT,
			updateStoreItemMapper: (item) => this.#updateTreeItemMapper(item),
		});
	}

	#updateTreeItemMapper = (item: UmbMediaTypeDetailModel) => {
		return {
			name: item.name,
			icon: item.icon,
		};
	};
}

export default UmbMediaTypeTreeStore;

export const UMB_MEDIA_TYPE_TREE_STORE_CONTEXT = new UmbContextToken<UmbMediaTypeTreeStore>('UmbMediaTypeTreeStore');
