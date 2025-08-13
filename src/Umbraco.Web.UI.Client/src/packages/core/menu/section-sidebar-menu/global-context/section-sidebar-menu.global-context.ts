import { UMB_SECTION_SIDEBAR_MENU_GLOBAL_CONTEXT } from './section-sidebar-menu.global-context.token.js';
import { UmbSectionSidebarMenuAppExpansionManager } from './expansion/section-sidebar-menu-app-expansion.manager.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbSectionSidebarMenuGlobalContext extends UmbContextBase {
	public readonly expansion = new UmbSectionSidebarMenuAppExpansionManager(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_SECTION_SIDEBAR_MENU_GLOBAL_CONTEXT);
	}
}

export { UmbSectionSidebarMenuGlobalContext as api };
