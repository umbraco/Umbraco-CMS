import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbWorkspaceContextInterface, UmbSaveableWorkspaceContextInterface } from '@umbraco-cms/backoffice/workspace';

export const UMB_SAVEABLE_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContextInterface,
	UmbSaveableWorkspaceContextInterface
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbSaveableWorkspaceContextInterface =>
		(context as UmbSaveableWorkspaceContextInterface).getIsNew !== undefined,
);
