import type { UmbSectionSidebarContext } from './section-sidebar.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_SECTION_SIDEBAR_CONTEXT = new UmbContextToken<UmbSectionSidebarContext>('UmbSectionSidebarContext');
