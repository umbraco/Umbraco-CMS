/* eslint-disable local-rules/no-direct-api-import */

import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	DocumentTypeService,
	type DocumentTypeTreeItemResponseModel,
	type PagedDocumentTypeTreeItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiTreeDataRequestManager } from '@umbraco-cms/backoffice/management-api';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeRootItemsRequestArgs,
} from '@umbraco-cms/backoffice/tree';

export class UmbManagementApiDocumentTypeTreeDataRequestManager extends UmbManagementApiTreeDataRequestManager<
	PagedDocumentTypeTreeItemResponseModel,
	PagedDocumentTypeTreeItemResponseModel,
	Array<DocumentTypeTreeItemResponseModel>
> {
	constructor(host: UmbControllerHost) {
		super(host, {
			getRootItems: (args: UmbTreeRootItemsRequestArgs) =>
				DocumentTypeService.getTreeDocumentTypeRoot({
					query: { foldersOnly: args.foldersOnly, skip: args.skip, take: args.take },
				}),

			getChildrenOf: (args: UmbTreeChildrenOfRequestArgs) =>
				DocumentTypeService.getTreeDocumentTypeChildren({
					query: { parentId: args.parent.unique, foldersOnly: args.foldersOnly, skip: args.skip, take: args.take },
				}),

			getAncestorsOf: (args: UmbTreeAncestorsOfRequestArgs) =>
				DocumentTypeService.getTreeDocumentTypeAncestors({
					query: { descendantId: args.treeItem.unique },
				}),
		});
	}
}
