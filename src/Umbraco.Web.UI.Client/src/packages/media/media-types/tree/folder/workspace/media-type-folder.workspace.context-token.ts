import { UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import type { UmbMediaTypeFolderWorkspaceContext } from './media-type-folder-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_MEDIA_TYPE_FOLDER_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContext,
	UmbMediaTypeFolderWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbMediaTypeFolderWorkspaceContext =>
		context.getEntityType?.() === UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE,
);
