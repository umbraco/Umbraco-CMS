import { UMB_CURRENT_USER_ENTITY_TYPE } from '../entity.js';
import type { UmbCurrentUserWorkspaceContext } from './current-user-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbSubmittableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_CURRENT_USER_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSubmittableWorkspaceContext,
	UmbCurrentUserWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbCurrentUserWorkspaceContext => context.getEntityType?.() === UMB_CURRENT_USER_ENTITY_TYPE,
);
