import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbContentTypeModel } from '@umbraco-cms/backoffice/content-type';
import type { UmbWorkspaceContext, UmbCollectionWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_COLLECTION_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContext,
	UmbCollectionWorkspaceContext<UmbContentTypeModel>
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbCollectionWorkspaceContext<UmbContentTypeModel> =>
		(context as UmbCollectionWorkspaceContext<UmbContentTypeModel>).contentTypeHasCollection !== undefined,
);

/**
 * @deprecated Use `UMB_COLLECTION_WORKSPACE_CONTEXT` instead. This token will be removed in the RC version.
 * TODO: Remove in RC
 */
export const UMB_WORKSPACE_COLLECTION_CONTEXT = UMB_COLLECTION_WORKSPACE_CONTEXT;
