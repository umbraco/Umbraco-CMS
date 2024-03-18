import type { UmbLanguageNavigationStructureWorkspaceContext } from './language-navigation-structure.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_LANGUAGE_NAVIGATION_STRUCTURE_WORKSPACE_CONTEXT =
	new UmbContextToken<UmbLanguageNavigationStructureWorkspaceContext>('UmbNavigationStructureWorkspaceContext');
