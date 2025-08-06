import { UMB_MENU_ITEM_CONTEXT } from './menu-item.context.token.js';
import { UmbEntityExpansionManager } from '@umbraco-cms/backoffice/utils';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDefaultMenuItemContext extends UmbContextBase {
	public readonly expansion = new UmbEntityExpansionManager(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_MENU_ITEM_CONTEXT);
	}
}
