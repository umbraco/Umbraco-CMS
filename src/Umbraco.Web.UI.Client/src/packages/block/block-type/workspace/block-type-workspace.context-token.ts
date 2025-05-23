import type { UmbBlockTypeWorkspaceContext } from './block-type-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_BLOCK_TYPE_WORKSPACE_CONTEXT = new UmbContextToken<UmbWorkspaceContext, UmbBlockTypeWorkspaceContext>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbBlockTypeWorkspaceContext => (context as any).IS_BLOCK_TYPE_WORKSPACE_CONTEXT,
);
