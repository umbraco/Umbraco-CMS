/* eslint-disable local-rules/no-direct-api-import */

import type { UmbElementTreeChildrenOfRequestArgs, UmbElementTreeRootItemsRequestArgs } from './types.js';
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

interface UmbManagementApiElementTreeRootItemsRequestArgs
	extends UmbManagementApiTreeRootItemsRequestArgs,
		Pick<UmbElementTreeRootItemsRequestArgs, 'dataType'> {}

interface UmbManagementApiElementTreeChildrenOfRequestArgs
	extends UmbManagementApiTreeChildrenOfRequestArgs,
		Pick<UmbElementTreeChildrenOfRequestArgs, 'dataType'> {}

interface UmbManagementApiElementTreeSiblingsFromRequestArgs extends UmbManagementApiTreeSiblingsFromRequestArgs {
	dataType?: {
		unique: string;
	};
}

export class UmbManagementApiElementTreeDataRequestManager extends UmbManagementApiTreeDataRequestManager<
	ElementTreeItemResponseModel,
	UmbManagementApiElementTreeRootItemsRequestArgs,
	PagedElementTreeItemResponseModel,
	UmbManagementApiElementTreeChildrenOfRequestArgs,
	PagedElementTreeItemResponseModel,
	UmbManagementApiTreeAncestorsOfRequestArgs,
	Array<ElementTreeItemResponseModel>,
	UmbManagementApiElementTreeSiblingsFromRequestArgs,
	SubsetElementTreeItemResponseModel
> {
	constructor(host: UmbControllerHost) {
		super(host, {
			getRootItems: (args) =>
				ElementService.getTreeElementRoot({
					query: {
						dataTypeId: args.dataType?.unique,
						foldersOnly: args.foldersOnly,
						skip: args.paging.skip,
						take: args.paging.take,
					},
				}),

			getChildrenOf: (args) =>
				ElementService.getTreeElementChildren({
					query: {
						parentId: args.parent.unique,
						dataTypeId: args.dataType?.unique,
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
						dataTypeId: args.dataType?.unique,
						foldersOnly: args.foldersOnly,
						target: args.paging.target.unique,
						before: args.paging.takeBefore,
						after: args.paging.takeAfter,
					},
				}),
		});
	}
}
