import type { UmbWorkspaceContext } from '../workspace-context.interface.js';
import type { UmbEntityNamedDetailWorkspaceContextBase } from './entity-named-detail-workspace-base.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_ENTITY_NAMED_DETAIL_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContext,
	UmbEntityNamedDetailWorkspaceContextBase
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbEntityNamedDetailWorkspaceContextBase =>
		(context as UmbEntityNamedDetailWorkspaceContextBase).IS_ENTITY_NAMED_DETAIL_WORKSPACE_CONTEXT,
);
