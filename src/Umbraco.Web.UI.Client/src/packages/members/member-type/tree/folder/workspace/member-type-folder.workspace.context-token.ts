import { UMB_MEMBER_TYPE_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import type { UmbMemberTypeFolderWorkspaceContext } from './member-type-folder-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_MEMBER_TYPE_FOLDER_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContext,
	UmbMemberTypeFolderWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbMemberTypeFolderWorkspaceContext =>
		context.getEntityType?.() === UMB_MEMBER_TYPE_FOLDER_ENTITY_TYPE,
);
