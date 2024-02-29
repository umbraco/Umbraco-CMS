import { UMB_TEMPLATE_DETAIL_STORE_CONTEXT } from '../repository/detail/template-detail.store.js';
import type { UmbTemplateDetailModel } from '../types.js';
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

		new UmbStoreConnector<UmbTemplateTreeItemModel, UmbTemplateDetailModel>(host, {
			store: this,
			connectToStoreAlias: UMB_TEMPLATE_DETAIL_STORE_CONTEXT,
			updateStoreItemMapper: (item) => this.#updateTreeItemMapper(item),
		});
	}

	#updateTreeItemMapper = (item: UmbTemplateDetailModel) => {
		return {
			name: item.name,
		};
	};
}

export default UmbTemplateTreeStore;

export const UMB_TEMPLATE_TREE_STORE_CONTEXT = new UmbContextToken<UmbTemplateTreeStore>('UmbTemplateTreeStore');
