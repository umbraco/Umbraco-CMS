import type { UmbMediaDetailModel } from '../types.js';
import { UMB_MEDIA_DETAIL_STORE_CONTEXT } from '../repository/detail/media-detail.store.js';
import type { UmbMediaTreeItemModel } from './types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbStoreConnector } from '@umbraco-cms/backoffice/store';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';

/**
 * @export
 * @class UmbMediaTreeStore
 * @extends {UmbUniqueTreeStore}
 * @description - Tree Data Store for Media Items
 */
export class UmbMediaTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbMediaTreeStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMediaTreeStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_MEDIA_TREE_STORE_CONTEXT.toString());

		new UmbStoreConnector<UmbMediaTreeItemModel, UmbMediaDetailModel>(host, {
			store: this,
			connectToStoreAlias: UMB_MEDIA_DETAIL_STORE_CONTEXT,
			updateStoreItemMapper: (item) => this.#updateTreeItemMapper(item),
		});
	}

	#updateTreeItemMapper = (item: UmbMediaDetailModel) => {
		return {
			name: item.variants[0].name,
		};
	};
}

export default UmbMediaTreeStore;

export const UMB_MEDIA_TREE_STORE_CONTEXT = new UmbContextToken<UmbMediaTreeStore>('UmbMediaTreeStore');
