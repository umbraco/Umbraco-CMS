import type { UmbMemberWorkspaceContext } from './member-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbSaveableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_MEMBER_WORKSPACE_CONTEXT = new UmbContextToken<UmbSaveableWorkspaceContext, UmbMemberWorkspaceContext>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbMemberWorkspaceContext => context.getEntityType?.() === 'member',
);
