import type { UmbWorkspaceContext } from '../workspace-context.interface.js';
import type { UmbEntityDetailWorkspaceContextBase } from './entity-detail-workspace-base.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContext,
	UmbEntityDetailWorkspaceContextBase
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbEntityDetailWorkspaceContextBase =>
		(context as UmbEntityDetailWorkspaceContextBase).IS_ENTITY_DETAIL_WORKSPACE_CONTEXT,
);
