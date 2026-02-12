import type { UmbSectionSidebarMenuGlobalContext } from './section-sidebar-menu.global-context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_SECTION_SIDEBAR_MENU_GLOBAL_CONTEXT = new UmbContextToken<UmbSectionSidebarMenuGlobalContext>(
	'UmbSectionSidebarMenuGlobalContext',
);
