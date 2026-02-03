import { UMB_ELEMENT_FOLDER_ENTITY_TYPE } from '../../entity.js';
import type { UmbElementFolderWorkspaceContext } from './element-folder-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_ELEMENT_FOLDER_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContext,
	UmbElementFolderWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbElementFolderWorkspaceContext =>
		context.getEntityType?.() === UMB_ELEMENT_FOLDER_ENTITY_TYPE,
);
