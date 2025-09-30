import { UMB_DATA_TYPE_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import type { UmbDataTypeFolderWorkspaceContext } from './data-type-folder-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_DATA_TYPE_FOLDER_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContext,
	UmbDataTypeFolderWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbDataTypeFolderWorkspaceContext =>
		context.getEntityType?.() === UMB_DATA_TYPE_FOLDER_ENTITY_TYPE,
);
