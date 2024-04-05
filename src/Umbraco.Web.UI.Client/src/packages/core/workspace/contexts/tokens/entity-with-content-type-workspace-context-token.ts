import type { UmbWorkspaceContext } from './workspace-context.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbEntityWithContentTypeWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_ENTITY_WITH_CONTENT_TYPE_WORKSPACE_CONTEXT = new UmbContextToken<UmbWorkspaceContext, UmbEntityWithContentTypeWorkspaceContext>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbEntityWithContentTypeWorkspaceContext => (context as any).contentTypeUnique !== undefined,
);
