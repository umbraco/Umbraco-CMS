import type { UmbMediaDetailModel } from '../types.js';
import { UMB_MEDIA_DETAIL_STORE_CONTEXT } from '../repository/detail/media-detail.store.js';
import type { UmbMediaTreeItemModel } from './types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
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
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbMediaTreeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEDIA_TREE_STORE_CONTEXT.toString());

		new UmbStoreConnector<UmbMediaTreeItemModel, UmbMediaDetailModel>(
			host,
			this,
			UMB_MEDIA_DETAIL_STORE_CONTEXT,
			(item) => this.#createTreeItemMapper(item),
			(item) => this.#updateTreeItemMapper(item),
		);
	}

	// TODO: revisit this when we have decided on detail model sizes
	// TODO: Fix tree model
	#createTreeItemMapper = (item: UmbMediaDetailModel) => {
		const treeItem: UmbMediaTreeItemModel = {
			unique: item.unique,
			parentUnique: item.parentUnique,
			name: item.variants[0].name,
			entityType: item.entityType,
			isFolder: false,
			hasChildren: false,
			variants: item.variants,
			isTrashed: item.isTrashed,
			mediaType: { unique: item.mediaType.unique, icon: '' },
			noAccess: false,
		};

		return treeItem;
	};

	// TODO: revisit this when we have decided on detail model sizes
	#updateTreeItemMapper = (item: UmbMediaDetailModel) => {
		return {
			name: item.variants[0].name,
		};
	};
}

export default UmbMediaTreeStore;

export const UMB_MEDIA_TREE_STORE_CONTEXT = new UmbContextToken<UmbMediaTreeStore>('UmbMediaTreeStore');
