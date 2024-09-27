import { UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import type { UmbDocumentBlueprintFolderWorkspaceContext } from './document-blueprint-folder-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_DOCUMENT_BLUEPRINT_FOLDER_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContext,
	UmbDocumentBlueprintFolderWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbDocumentBlueprintFolderWorkspaceContext =>
		context.getEntityType?.() === UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE,
);
