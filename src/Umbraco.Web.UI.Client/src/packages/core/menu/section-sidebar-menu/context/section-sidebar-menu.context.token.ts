import type { UmbSectionSidebarMenuContext } from './section-sidebar-menu.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_SECTION_SIDEBAR_MENU_CONTEXT = new UmbContextToken<UmbSectionSidebarMenuContext>(
	'UmbSectionSidebarMenuContext',
);
