/* eslint-disable local-rules/no-direct-api-import */

import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	MediaTypeService,
	type MediaTypeTreeItemResponseModel,
	type PagedMediaTypeTreeItemResponseModel,
	type SubsetMediaTypeTreeItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import {
	UmbManagementApiTreeDataRequestManager,
	type UmbManagementApiTreeAncestorsOfRequestArgs,
	type UmbManagementApiTreeChildrenOfRequestArgs,
	type UmbManagementApiTreeRootItemsRequestArgs,
	type UmbManagementApiTreeSiblingsFromRequestArgs,
} from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiMediaTypeTreeDataRequestManager extends UmbManagementApiTreeDataRequestManager<
	MediaTypeTreeItemResponseModel,
	UmbManagementApiTreeRootItemsRequestArgs,
	PagedMediaTypeTreeItemResponseModel,
	UmbManagementApiTreeChildrenOfRequestArgs,
	PagedMediaTypeTreeItemResponseModel,
	UmbManagementApiTreeAncestorsOfRequestArgs,
	Array<MediaTypeTreeItemResponseModel>,
	UmbManagementApiTreeSiblingsFromRequestArgs,
	SubsetMediaTypeTreeItemResponseModel
> {
	constructor(host: UmbControllerHost) {
		super(host, {
			getRootItems: (args) =>
				MediaTypeService.getTreeMediaTypeRoot({
					query: {
						foldersOnly: args.foldersOnly,
						skip: args.paging.skip,
						take: args.paging.take,
					},
				}),

			getChildrenOf: (args) =>
				MediaTypeService.getTreeMediaTypeChildren({
					query: {
						parentId: args.parent.unique,
						foldersOnly: args.foldersOnly,
						skip: args.paging.skip,
						take: args.paging.take,
					},
				}),

			getAncestorsOf: (args) =>
				MediaTypeService.getTreeMediaTypeAncestors({
					query: {
						descendantId: args.treeItem.unique,
					},
				}),

			getSiblingsFrom: (args) =>
				MediaTypeService.getTreeMediaTypeSiblings({
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
