import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbWorkspaceContext, UmbSubmittableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_SUBMITTABLE_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContext,
	UmbSubmittableWorkspaceContext
>('UmbWorkspaceContext', undefined, (context): context is UmbSubmittableWorkspaceContext => 'requestSubmit' in context);
