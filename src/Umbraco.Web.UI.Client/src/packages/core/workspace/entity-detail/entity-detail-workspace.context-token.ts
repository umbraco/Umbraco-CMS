import type { UmbEntityDetailWorkspaceContextBase } from './entity-detail-workspace-base.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT = new UmbContextToken<UmbEntityDetailWorkspaceContextBase>(
	'UmbWorkspaceContext',
);
