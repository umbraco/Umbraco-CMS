import type { UmbWorkspaceContext } from '../../types.js';
import type { UmbSubmittableWorkspaceContext } from './submittable-workspace-context.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_SUBMITTABLE_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContext,
	UmbSubmittableWorkspaceContext
>('UmbWorkspaceContext', undefined, (context): context is UmbSubmittableWorkspaceContext => 'requestSubmit' in context);
