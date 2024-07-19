import type { UmbMemberGroupWorkspaceContext } from './member-group-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbSubmittableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_MEMBER_GROUP_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSubmittableWorkspaceContext,
	UmbMemberGroupWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbMemberGroupWorkspaceContext => context.getEntityType?.() === 'member-group',
);
