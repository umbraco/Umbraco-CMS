import type { UmbMenuVariantStructureWorkspaceContext } from './menu-variant-structure-workspace-context.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_MENU_VARIANT_STRUCTURE_WORKSPACE_CONTEXT =
	new UmbContextToken<UmbMenuVariantStructureWorkspaceContext>(
		'UmbWorkspaceContext',
		'UmbMenuStructure',
		(context): context is UmbMenuVariantStructureWorkspaceContext =>
			'IS_MENU_VARIANT_STRUCTURE_WORKSPACE_CONTEXT' in context,
	);
