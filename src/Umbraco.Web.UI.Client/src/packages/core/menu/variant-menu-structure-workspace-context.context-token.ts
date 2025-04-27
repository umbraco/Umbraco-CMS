import type { UmbVariantMenuStructureWorkspaceContext } from './variant-menu-structure-workspace-context.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_VARIANT_MENU_STRUCTURE_WORKSPACE_CONTEXT =
	new UmbContextToken<UmbVariantMenuStructureWorkspaceContext>(
		'UmbWorkspaceContext',
		'UmbMenuStructure',
		(context): context is UmbVariantMenuStructureWorkspaceContext =>
			'IS_VARIANT_MENU_STRUCTURE_WORKSPACE_CONTEXT' in context,
	);
