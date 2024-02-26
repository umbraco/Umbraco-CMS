import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type {
	UmbPublishableWorkspaceContextInterface,
	UmbSaveableWorkspaceContextInterface,
} from '@umbraco-cms/backoffice/workspace';

export const UMB_PUBLISHABLE_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSaveableWorkspaceContextInterface,
	UmbPublishableWorkspaceContextInterface
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbPublishableWorkspaceContextInterface => (context as any).publish !== undefined,
);
