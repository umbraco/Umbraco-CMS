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

		new UmbStoreConnector<UmbMediaTypeTreeItemModel, UmbMediaTypeDetailModel>(
			host,
			this,
			UMB_MEDIA_TYPE_DETAIL_STORE_CONTEXT,
			(item) => this.#createTreeItemMapper(item),
			(item) => this.#updateTreeItemMapper(item),
		);
	}

	// TODO: revisit this when we have decided on detail model sizes
	#createTreeItemMapper = (item: UmbMediaTypeDetailModel) => {
		const treeItem: UmbMediaTypeTreeItemModel = {
			unique: item.unique,
			parentUnique: null,
			name: item.name,
			entityType: item.entityType,
			isFolder: false,
			hasChildren: false,
		};

		return treeItem;
	};

	// TODO: revisit this when we have decided on detail model sizes
	#updateTreeItemMapper = (item: UmbMediaTypeDetailModel) => {
		return {
			name: item.name,
		};
	};
}

export const UMB_MEDIA_TYPE_TREE_STORE_CONTEXT = new UmbContextToken<UmbMediaTypeTreeStore>('UmbMediaTypeTreeStore');
