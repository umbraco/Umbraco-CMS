import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbWorkspaceContext, UmbSaveableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_SAVEABLE_WORKSPACE_CONTEXT = new UmbContextToken<UmbWorkspaceContext, UmbSaveableWorkspaceContext>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbSaveableWorkspaceContext => 'getIsNew' in context,
);
