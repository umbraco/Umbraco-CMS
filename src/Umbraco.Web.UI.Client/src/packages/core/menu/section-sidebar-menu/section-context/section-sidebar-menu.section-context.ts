import { UmbSectionSidebarMenuSectionExpansionManager } from './expansion/section-sidebar-menu-section-expansion.manager.js';
import { UMB_SECTION_SIDEBAR_MENU_SECTION_CONTEXT } from './section-sidebar-menu.section-context.token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbSectionSidebarMenuSectionContext extends UmbContextBase {
	public readonly expansion = new UmbSectionSidebarMenuSectionExpansionManager(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_SECTION_SIDEBAR_MENU_SECTION_CONTEXT);
	}
}

export { UmbSectionSidebarMenuSectionContext as api };
