import { UMB_SECTION_SIDEBAR_MENU_CONTEXT } from '../../section-sidebar-menu/context/section-sidebar-menu.context.token.js';
import { UMB_MENU_CONTEXT } from './menu.context.token.js';
import { UmbEntityExpansionManager } from '@umbraco-cms/backoffice/utils';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDefaultMenuContext extends UmbContextBase {
	public readonly expansion = new UmbEntityExpansionManager(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_MENU_CONTEXT);

		// TODO: remove this dependency on the section sidebar menu context
		// This is a temporary solution to allow the menu to work in the section sidebar menu
		this.consumeContext(UMB_SECTION_SIDEBAR_MENU_CONTEXT, (context) => {
			this.#observeExpansion(context);
		});
	}

	#observeExpansion(context: typeof UMB_SECTION_SIDEBAR_MENU_CONTEXT.TYPE | undefined) {
		this.observe(context?.expansion.expansion, (items) => {
			this.expansion.setExpansion(items || []);
		});
	}
}
