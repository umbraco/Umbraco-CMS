import { UMB_ELEMENT_ENTITY_TYPE } from '../entity.js';
import type { UmbElementWorkspaceContext } from './element-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbSubmittableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_ELEMENT_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSubmittableWorkspaceContext,
	UmbElementWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbElementWorkspaceContext => context.getEntityType?.() === UMB_ELEMENT_ENTITY_TYPE,
);
