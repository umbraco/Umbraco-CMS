import type { UmbContentCollectionWorkspaceContext } from './content-collection-workspace-context.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbContentTypeModel } from '@umbraco-cms/backoffice/content-type';
import type { UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

/**
 * @deprecated Deprecated since v18. Use `UMB_CONTENT_COLLECTION_CONFIGURATION_CONTEXT` instead, which answers the
 * collection configuration from any host rather than only a workspace. Scheduled for removal in Umbraco 20.
 */
export const UMB_CONTENT_COLLECTION_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContext,
	UmbContentCollectionWorkspaceContext<UmbContentTypeModel>
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbContentCollectionWorkspaceContext<UmbContentTypeModel> =>
		(context as UmbContentCollectionWorkspaceContext<UmbContentTypeModel>).collection?.hasCollection !== undefined,
);
