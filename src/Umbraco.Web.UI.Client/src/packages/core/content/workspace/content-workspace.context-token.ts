import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbContentWorkspaceContext } from './content-workspace-context.interface.js';

export const UMB_CONTENT_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbContentWorkspaceContext,
	UmbContentWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbContentWorkspaceContext => (context as any).IS_CONTENT_WORKSPACE_CONTEXT,
);
