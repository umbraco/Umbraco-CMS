import type UmbBlockGridAreaTypeWorkspaceContext from './block-grid-area-type-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_BLOCK_GRID_AREA_TYPE_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContext,
	UmbBlockGridAreaTypeWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbBlockGridAreaTypeWorkspaceContext =>
		(context as any).IS_BLOCK_GRID_AREA_TYPE_WORKSPACE_CONTEXT,
);
