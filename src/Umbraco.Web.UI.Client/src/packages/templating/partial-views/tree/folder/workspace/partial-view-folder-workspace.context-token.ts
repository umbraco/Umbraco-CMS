import { UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import type { UmbPartialViewFolderWorkspaceContext } from './partial-view-folder-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_PARTIAL_VIEW_FOLDER_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContext,
	UmbPartialViewFolderWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbPartialViewFolderWorkspaceContext =>
		context.getEntityType?.() === UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE,
);
