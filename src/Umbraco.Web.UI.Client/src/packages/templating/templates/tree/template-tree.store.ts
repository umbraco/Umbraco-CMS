import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
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
	}
}

export const UMB_TEMPLATE_TREE_STORE_CONTEXT = new UmbContextToken<UmbTemplateTreeStore>('UmbTemplateTreeStore');
