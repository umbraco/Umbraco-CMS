import type { UmbWorkspaceContext } from '../../workspace-context.interface.js';
import type { UmbTreeEntityWorkspaceContext } from './tree-entity-workspace-context.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_TREE_ENTITY_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContext,
	UmbTreeEntityWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbTreeEntityWorkspaceContext => 'parentEntityType' in context,
);
