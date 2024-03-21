import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbContentTypeModel } from '@umbraco-cms/backoffice/content-type';
import type {
	UmbWorkspaceContextInterface,
	UmbWorkspaceCollectionContextInterface,
} from '@umbraco-cms/backoffice/workspace';

export const UMB_WORKSPACE_COLLECTION_CONTEXT = new UmbContextToken<
	UmbWorkspaceContextInterface,
	UmbWorkspaceCollectionContextInterface<UmbContentTypeModel>
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbWorkspaceCollectionContextInterface<UmbContentTypeModel> =>
		(context as UmbWorkspaceCollectionContextInterface<UmbContentTypeModel>).contentTypeHasCollection !== undefined,
);
