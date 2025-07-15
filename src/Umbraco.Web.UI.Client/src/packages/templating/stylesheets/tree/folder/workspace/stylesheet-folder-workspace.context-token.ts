import { UMB_STYLESHEET_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import type { UmbStylesheetFolderWorkspaceContext } from './stylesheet-folder-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_STYLESHEET_FOLDER_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContext,
	UmbStylesheetFolderWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbStylesheetFolderWorkspaceContext =>
		context.getEntityType?.() === UMB_STYLESHEET_FOLDER_ENTITY_TYPE,
);
