import { UMB_COLLECTION_MENU_CONTEXT } from './default-collection-menu.context.token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDefaultCollectionMenuContext extends UmbContextBase {
	constructor(host: UmbControllerHost) {
		super(host, UMB_COLLECTION_MENU_CONTEXT);
		debugger;
	}
}

export { UmbDefaultCollectionMenuContext as api };
