import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbContentTypeModel } from '@umbraco-cms/backoffice/content-type';
import type {
	UmbWorkspaceContextInterface,
	UmbCollectionWorkspaceContextInterface,
} from '@umbraco-cms/backoffice/workspace';

export const UMB_WORKSPACE_COLLECTION_CONTEXT = new UmbContextToken<
	UmbWorkspaceContextInterface,
	UmbCollectionWorkspaceContextInterface<UmbContentTypeModel>
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbCollectionWorkspaceContextInterface<UmbContentTypeModel> =>
		(context as UmbCollectionWorkspaceContextInterface<UmbContentTypeModel>).contentTypeHasCollection !== undefined,
);
