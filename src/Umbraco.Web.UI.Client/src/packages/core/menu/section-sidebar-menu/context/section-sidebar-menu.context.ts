import { UMB_SECTION_SIDEBAR_MENU_CONTEXT } from './section-sidebar-menu.context.token.js';
import { UmbEntityExpansionManager } from '@umbraco-cms/backoffice/utils';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbSectionSidebarMenuContext extends UmbContextBase {
	public readonly expansion = new UmbEntityExpansionManager(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_SECTION_SIDEBAR_MENU_CONTEXT);
	}
}
