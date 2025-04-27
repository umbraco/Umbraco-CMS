import type { UmbMenuStructureWorkspaceContext } from './menu-structure-workspace-context.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_MENU_STRUCTURE_WORKSPACE_CONTEXT = new UmbContextToken<UmbMenuStructureWorkspaceContext>(
	'UmbWorkspaceContext',
	'UmbMenuStructure',
);
