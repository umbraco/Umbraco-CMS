import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbTreeStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHostInterface } from '@umbraco-cms/backoffice/controller';

export const UMB_TEMPLATE_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbTemplateTreeStore>(
	'UmbTemplateTreeStore'
);

/**
 * @export
 * @class UmbTemplateTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for Templates
 */
export class UmbTemplateTreeStore extends UmbTreeStoreBase {
	/**
	 * Creates an instance of UmbTemplateTreeStore.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbTemplateTreeStore
	 */
	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_TEMPLATE_TREE_STORE_CONTEXT_TOKEN.toString());
	}
}
