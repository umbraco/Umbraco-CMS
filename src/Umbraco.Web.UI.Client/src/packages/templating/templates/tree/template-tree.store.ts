import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
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
	 * @param {UmbControllerHost} host
	 * @memberof UmbTemplateTreeStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_TEMPLATE_TREE_STORE_CONTEXT.toString());
	}
}

export default UmbTemplateTreeStore;

export const UMB_TEMPLATE_TREE_STORE_CONTEXT = new UmbContextToken<UmbTemplateTreeStore>('UmbTemplateTreeStore');
