import { UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE } from '../entity.js';
import type { UmbDocumentTypeFolderWorkspaceContext } from './document-type-folder-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_DOCUMENT_TYPE_FOLDER_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContext,
	UmbDocumentTypeFolderWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbDocumentTypeFolderWorkspaceContext =>
		context.getEntityType?.() === UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE,
);
