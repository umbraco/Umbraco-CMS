import type { UmbWorkspaceContext } from '../../types.js';
import type { UmbSubmittableTreeEntityWorkspaceContext } from './submittable-tree-entity-workspace-context.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_SUBMITTABLE_TREE_ENTITY_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContext,
	UmbSubmittableTreeEntityWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbSubmittableTreeEntityWorkspaceContext =>
		'requestSubmit' in context && 'createUnderParent' in context,
);
