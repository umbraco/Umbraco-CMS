import { UMB_PARTIAL_VIEW_ENTITY_TYPE } from '../entity.js';
import { UMB_PARTIAL_VIEW_DETAIL_STORE_CONTEXT } from '../repository/partial-view-detail.store.js';
import { UmbPartialViewDetailModel } from '../types.js';
import { UmbPartialViewTreeItemModel } from './types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
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
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbPartialViewTreeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_PARTIAL_VIEW_TREE_STORE_CONTEXT.toString());

		new UmbStoreConnector<UmbPartialViewTreeItemModel, UmbPartialViewDetailModel>(
			host,
			this,
			UMB_PARTIAL_VIEW_DETAIL_STORE_CONTEXT,
			(item) => this.#createTreeItemMapper(item),
			(item) => this.#updateTreeItemMapper(item),
		);
	}

	// TODO: revisit this when we have decided on detail model sizes
	#createTreeItemMapper = (item: UmbPartialViewDetailModel) => {
		const treeItem: UmbPartialViewTreeItemModel = {
			unique: item.unique,
			parentUnique: item.parentUnique,
			entityType: UMB_PARTIAL_VIEW_ENTITY_TYPE,
			name: item.name,
			hasChildren: false,
			isContainer: false,
			isFolder: false,
		};

		return treeItem;
	};

	// TODO: revisit this when we have decided on detail model sizes
	#updateTreeItemMapper = (item: UmbPartialViewDetailModel) => {
		return {
			name: item.name,
		};
	};
}

export const UMB_PARTIAL_VIEW_TREE_STORE_CONTEXT = new UmbContextToken<UmbPartialViewTreeStore>(
	'UmbPartialViewTreeStore',
);
