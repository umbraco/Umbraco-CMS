import type { UmbWorkspaceContext } from '../../workspace-context.interface.js';
import type { UmbVariantTreeEntityWorkspaceContext } from './variant-tree-entity-workspace-context.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_VARIANT_TREE_ENTITY_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContext,
	UmbVariantTreeEntityWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbVariantTreeEntityWorkspaceContext => 'variants' in context && 'parentEntityType' in context,
);
