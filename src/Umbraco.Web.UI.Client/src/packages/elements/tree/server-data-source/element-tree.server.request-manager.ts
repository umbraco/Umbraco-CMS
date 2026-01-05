/* eslint-disable local-rules/no-direct-api-import */

import { ElementService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiTreeDataRequestManager } from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	ElementTreeItemResponseModel,
	PagedElementTreeItemResponseModel,
	SubsetElementTreeItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import type {
	UmbManagementApiTreeAncestorsOfRequestArgs,
	UmbManagementApiTreeChildrenOfRequestArgs,
	UmbManagementApiTreeRootItemsRequestArgs,
	UmbManagementApiTreeSiblingsFromRequestArgs,
} from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiElementTreeDataRequestManager extends UmbManagementApiTreeDataRequestManager<
	ElementTreeItemResponseModel,
	UmbManagementApiTreeRootItemsRequestArgs,
	PagedElementTreeItemResponseModel,
	UmbManagementApiTreeChildrenOfRequestArgs,
	PagedElementTreeItemResponseModel,
	UmbManagementApiTreeAncestorsOfRequestArgs,
	Array<ElementTreeItemResponseModel>,
	UmbManagementApiTreeSiblingsFromRequestArgs,
	SubsetElementTreeItemResponseModel
> {
	constructor(host: UmbControllerHost) {
		super(host, {
			getRootItems: (args) =>
				ElementService.getTreeElementRoot({
					query: {
						foldersOnly: args.foldersOnly,
						skip: args.paging.skip,
						take: args.paging.take,
					},
				}),

			getChildrenOf: (args) =>
				ElementService.getTreeElementChildren({
					query: {
						parentId: args.parent.unique,
						foldersOnly: args.foldersOnly,
						skip: args.paging.skip,
						take: args.paging.take,
					},
				}),

			getAncestorsOf: (args) =>
				ElementService.getTreeElementAncestors({
					query: {
						descendantId: args.treeItem.unique,
					},
				}),

			getSiblingsFrom: (args) =>
				ElementService.getTreeElementSiblings({
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
