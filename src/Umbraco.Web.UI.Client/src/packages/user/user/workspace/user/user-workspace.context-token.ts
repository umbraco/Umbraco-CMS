import { UMB_USER_ENTITY_TYPE } from '../../entity.js';
import type { UmbUserWorkspaceContext } from './user-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbSubmittableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_USER_WORKSPACE_CONTEXT = new UmbContextToken<UmbSubmittableWorkspaceContext, UmbUserWorkspaceContext>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbUserWorkspaceContext => context.getEntityType?.() === UMB_USER_ENTITY_TYPE,
);
