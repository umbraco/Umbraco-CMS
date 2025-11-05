import { UMB_MENU_CONTEXT } from './menu.context.token.js';
import type { UmbMenuItemExpansionEntryModel } from './types.js';
import { UmbEntityExpansionManager } from '@umbraco-cms/backoffice/utils';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDefaultMenuContext extends UmbContextBase {
	public readonly expansion = new UmbEntityExpansionManager<UmbMenuItemExpansionEntryModel>(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_MENU_CONTEXT);
	}
}
