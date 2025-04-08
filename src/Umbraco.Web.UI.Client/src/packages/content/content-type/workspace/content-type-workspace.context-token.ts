import type { UmbContentTypeWorkspaceContext } from './content-type-workspace-context.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_CONTENT_TYPE_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbContentTypeWorkspaceContext,
	UmbContentTypeWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbContentTypeWorkspaceContext => (context as any).IS_CONTENT_TYPE_WORKSPACE_CONTEXT,
);
