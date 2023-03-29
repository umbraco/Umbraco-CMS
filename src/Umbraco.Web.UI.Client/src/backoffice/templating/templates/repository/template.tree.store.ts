import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbEntityTreeStore } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

export const UMB_TEMPLATE_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbTemplateTreeStore>('UmbTemplateTreeStore');

/**
 * @export
 * @class UmbTemplateTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for Templates
 */
export class UmbTemplateTreeStore extends UmbEntityTreeStore {
	/**
	 * Creates an instance of UmbTemplateTreeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbTemplateTreeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_TEMPLATE_TREE_STORE_CONTEXT_TOKEN.toString());
	}
}
