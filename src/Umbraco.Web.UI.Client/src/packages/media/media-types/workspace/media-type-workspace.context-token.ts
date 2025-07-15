import type { UmbMediaTypeWorkspaceContext } from './media-type-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbSubmittableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_MEDIA_TYPE_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSubmittableWorkspaceContext,
	UmbMediaTypeWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbMediaTypeWorkspaceContext => context.getEntityType?.() === 'media-type',
);
