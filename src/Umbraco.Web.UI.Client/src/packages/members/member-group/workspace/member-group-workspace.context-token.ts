import type { UmbMemberGroupWorkspaceContext } from './member-group-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbSaveableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_MEMBER_GROUP_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSaveableWorkspaceContext,
	UmbMemberGroupWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbMemberGroupWorkspaceContext => context.getEntityType?.() === 'member-group',
);
