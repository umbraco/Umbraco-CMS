import type { UmbBlockWorkspaceContext } from './block-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbWorkspaceContextInterface } from '@umbraco-cms/backoffice/workspace';

export const UMB_BLOCK_WORKSPACE_CONTEXT = new UmbContextToken<UmbWorkspaceContextInterface, UmbBlockWorkspaceContext>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbBlockWorkspaceContext => (context as any).IS_BLOCK_WORKSPACE_CONTEXT,
);
