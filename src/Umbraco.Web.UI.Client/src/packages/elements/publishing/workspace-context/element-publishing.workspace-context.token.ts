import type { UmbElementPublishingWorkspaceContext } from './element-publishing.workspace-context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_ELEMENT_PUBLISHING_WORKSPACE_CONTEXT = new UmbContextToken<UmbElementPublishingWorkspaceContext>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbElementPublishingWorkspaceContext =>
		(context as UmbElementPublishingWorkspaceContext).saveAndPublish !== undefined,
);
