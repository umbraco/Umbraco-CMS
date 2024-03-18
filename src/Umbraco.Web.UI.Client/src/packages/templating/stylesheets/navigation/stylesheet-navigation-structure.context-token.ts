import type { UmbStylesheetNavigationStructureWorkspaceContext } from './stylesheet-navigation-structure.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_STYLESHEET_NAVIGATION_STRUCTURE_WORKSPACE_CONTEXT =
	new UmbContextToken<UmbStylesheetNavigationStructureWorkspaceContext>('UmbNavigationStructureWorkspaceContext');
