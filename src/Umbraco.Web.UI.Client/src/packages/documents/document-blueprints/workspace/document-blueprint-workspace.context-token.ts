import { UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE } from '../entity.js';
import type { UmbDocumentBlueprintWorkspaceContext } from './document-blueprint-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbSubmittableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_DOCUMENT_BLUEPRINT_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSubmittableWorkspaceContext,
	UmbDocumentBlueprintWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbDocumentBlueprintWorkspaceContext =>
		context.getEntityType?.() === UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE,
);
