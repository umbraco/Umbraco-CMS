import { UMB_PARTIAL_VIEW_ENTITY_TYPE } from '../entity.js';
import type { UmbPartialViewWorkspaceContext } from './partial-view-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbSubmittableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_PARTIAL_VIEW_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSubmittableWorkspaceContext,
	UmbPartialViewWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbPartialViewWorkspaceContext => context.getEntityType?.() === UMB_PARTIAL_VIEW_ENTITY_TYPE,
);
