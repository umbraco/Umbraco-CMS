import { UMB_TEMPLATE_DETAIL_STORE_CONTEXT } from '../repository/detail/template-detail.store.js';
import type { UmbTemplateDetailModel } from '../types.js';
import { UMB_TEMPLATE_ENTITY_TYPE } from '../entity.js';
import type { UmbTemplateTreeItemModel } from './types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbStoreConnector } from '@umbraco-cms/backoffice/store';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';

/**
 * @export
 * @class UmbTemplateTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for Template Items
 */
export class UmbTemplateTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbTemplateTreeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbTemplateTreeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_TEMPLATE_TREE_STORE_CONTEXT.toString());

		new UmbStoreConnector<UmbTemplateTreeItemModel, UmbTemplateDetailModel>(
			host,
			this,
			UMB_TEMPLATE_DETAIL_STORE_CONTEXT,
			(item) => this.#createTreeItemMapper(item),
			(item) => this.#updateTreeItemMapper(item),
		);
	}

	// TODO: revisit this when we have decided on detail model sizes
	#createTreeItemMapper = (item: UmbTemplateDetailModel) => {
		const treeItem: UmbTemplateTreeItemModel = {
			unique: item.unique,
			parentUnique: item.parentUnique,
			entityType: UMB_TEMPLATE_ENTITY_TYPE,
			name: item.name,
			hasChildren: false,
			isFolder: false,
		};

		return treeItem;
	};

	// TODO: revisit this when we have decided on detail model sizes
	#updateTreeItemMapper = (item: UmbTemplateDetailModel) => {
		return {
			name: item.name,
		};
	};
}

export const UMB_TEMPLATE_TREE_STORE_CONTEXT = new UmbContextToken<UmbTemplateTreeStore>('UmbTemplateTreeStore');
