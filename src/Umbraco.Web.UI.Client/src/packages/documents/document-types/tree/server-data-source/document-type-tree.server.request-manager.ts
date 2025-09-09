/* eslint-disable local-rules/no-direct-api-import */

import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	DocumentTypeService,
	type DocumentTypeTreeItemResponseModel,
	type PagedDocumentTypeTreeItemResponseModel,
	type SubsetDocumentTypeTreeItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import {
	UmbManagementApiTreeDataRequestManager,
	type UmbManagementApiTreeAncestorsOfRequestArgs,
	type UmbManagementApiTreeChildrenOfRequestArgs,
	type UmbManagementApiTreeRootItemsRequestArgs,
	type UmbManagementApiTreeSiblingsFromRequestArgs,
} from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiDocumentTypeTreeDataRequestManager extends UmbManagementApiTreeDataRequestManager<
	DocumentTypeTreeItemResponseModel,
	UmbManagementApiTreeRootItemsRequestArgs,
	PagedDocumentTypeTreeItemResponseModel,
	UmbManagementApiTreeChildrenOfRequestArgs,
	PagedDocumentTypeTreeItemResponseModel,
	UmbManagementApiTreeAncestorsOfRequestArgs,
	Array<DocumentTypeTreeItemResponseModel>,
	UmbManagementApiTreeSiblingsFromRequestArgs,
	SubsetDocumentTypeTreeItemResponseModel
> {
	constructor(host: UmbControllerHost) {
		super(host, {
			getRootItems: (args) =>
				DocumentTypeService.getTreeDocumentTypeRoot({
					query: {
						foldersOnly: args.foldersOnly,
						skip: args.paging.skip,
						take: args.paging.take,
					},
				}),

			getChildrenOf: (args) =>
				DocumentTypeService.getTreeDocumentTypeChildren({
					query: {
						parentId: args.parent.unique,
						foldersOnly: args.foldersOnly,
						skip: args.paging.skip,
						take: args.paging.take,
					},
				}),

			getAncestorsOf: (args) =>
				DocumentTypeService.getTreeDocumentTypeAncestors({
					query: {
						descendantId: args.treeItem.unique,
					},
				}),

			getSiblingsFrom: (args) =>
				DocumentTypeService.getTreeDocumentTypeSiblings({
					query: {
						foldersOnly: args.foldersOnly,
						target: args.paging.target.unique,
						before: args.paging.takeBefore,
						after: args.paging.takeAfter,
					},
				}),
		});
	}
}
