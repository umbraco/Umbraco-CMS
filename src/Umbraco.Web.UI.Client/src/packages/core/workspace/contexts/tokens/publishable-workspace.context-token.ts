import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbPublishableWorkspaceContext, UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_PUBLISHABLE_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContext,
	UmbPublishableWorkspaceContext
>('UmbWorkspaceContext', undefined, (context): context is UmbPublishableWorkspaceContext => 'publish' in context);
