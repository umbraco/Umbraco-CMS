import type { UmbEntityExpansionSectionEntryModel } from '../types.js';
import { UMB_SECTION_SIDEBAR_MENU_GLOBAL_CONTEXT } from './section-sidebar-menu.global-context.token.js';
import { UmbEntityExpansionManager } from '@umbraco-cms/backoffice/utils';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbSectionSidebarMenuGlobalContext extends UmbContextBase {
	public readonly expansion = new UmbEntityExpansionManager<UmbEntityExpansionSectionEntryModel>(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_SECTION_SIDEBAR_MENU_GLOBAL_CONTEXT);
	}
}

export { UmbSectionSidebarMenuGlobalContext as api };
