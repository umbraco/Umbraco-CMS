import { UMB_SCRIPT_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import type { UmbScriptFolderWorkspaceContext } from './script-folder-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_SCRIPT_FOLDER_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContext,
	UmbScriptFolderWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbScriptFolderWorkspaceContext => context.getEntityType?.() === UMB_SCRIPT_FOLDER_ENTITY_TYPE,
);
