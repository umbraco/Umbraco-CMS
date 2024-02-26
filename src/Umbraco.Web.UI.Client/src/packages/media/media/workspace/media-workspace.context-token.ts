import { UMB_MEDIA_ENTITY_TYPE } from '../entity.js';
import type { UmbMediaWorkspaceContext } from './media-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbSaveableWorkspaceContextInterface } from '@umbraco-cms/backoffice/workspace';

export const UMB_MEDIA_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSaveableWorkspaceContextInterface,
	UmbMediaWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbMediaWorkspaceContext => context.getEntityType?.() === UMB_MEDIA_ENTITY_TYPE,
);
