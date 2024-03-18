import type { UmbScriptNavigationStructureWorkspaceContext } from './script-navigation-structure.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_SCRIPT_NAVIGATION_STRUCTURE_WORKSPACE_CONTEXT =
	new UmbContextToken<UmbScriptNavigationStructureWorkspaceContext>('UmbNavigationStructureWorkspaceContext');
