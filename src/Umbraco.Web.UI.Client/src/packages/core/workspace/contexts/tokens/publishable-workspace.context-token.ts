import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type {
	UmbPublishableWorkspaceContextInterface,
	UmbWorkspaceContextInterface,
} from '@umbraco-cms/backoffice/workspace';

export const UMB_PUBLISHABLE_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContextInterface,
	UmbPublishableWorkspaceContextInterface
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbPublishableWorkspaceContextInterface => 'publish' in context,
);
