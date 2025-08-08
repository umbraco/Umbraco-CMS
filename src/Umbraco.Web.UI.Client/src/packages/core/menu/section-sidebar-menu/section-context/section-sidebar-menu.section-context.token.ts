import type { UmbSectionSidebarMenuSectionContext } from './section-sidebar-menu.section-context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_SECTION_SIDEBAR_MENU_SECTION_CONTEXT = new UmbContextToken<UmbSectionSidebarMenuSectionContext>(
	'UmbSectionSidebarMenuSectionContext',
);
