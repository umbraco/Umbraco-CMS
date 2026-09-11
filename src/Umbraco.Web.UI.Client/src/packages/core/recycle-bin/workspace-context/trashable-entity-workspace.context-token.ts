import type { UmbTrashableEntityWorkspaceContext } from './types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_TRASHABLE_ENTITY_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContext,
	UmbTrashableEntityWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbTrashableEntityWorkspaceContext =>
		'readOnlyGuard' in context && 'reload' in context && 'isTrashed' in context && 'isNew' in context,
);
