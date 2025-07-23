import type { UmbWorkspaceContext } from '../../types.js';
import type { UmbPublishableWorkspaceContext } from './publishable-workspace-context.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_PUBLISHABLE_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContext,
	UmbPublishableWorkspaceContext
>('UmbWorkspaceContext', undefined, (context): context is UmbPublishableWorkspaceContext => 'publish' in context);
