/* eslint-disable local-rules/no-direct-api-import */
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { DataTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiTreeDataRequestManager } from '@umbraco-cms/backoffice/management-api';
import type {
	DataTypeTreeItemResponseModel,
	PagedDataTypeTreeItemResponseModel,
	SubsetDataTypeTreeItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import type {
	UmbManagementApiTreeAncestorsOfRequestArgs,
	UmbManagementApiTreeChildrenOfRequestArgs,
	UmbManagementApiTreeRootItemsRequestArgs,
	UmbManagementApiTreeSiblingsFromRequestArgs,
} from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiDataTypeTreeDataRequestManager extends UmbManagementApiTreeDataRequestManager<
	DataTypeTreeItemResponseModel,
	UmbManagementApiTreeRootItemsRequestArgs,
	PagedDataTypeTreeItemResponseModel,
	UmbManagementApiTreeChildrenOfRequestArgs,
	PagedDataTypeTreeItemResponseModel,
	UmbManagementApiTreeAncestorsOfRequestArgs,
	Array<DataTypeTreeItemResponseModel>,
	UmbManagementApiTreeSiblingsFromRequestArgs,
	SubsetDataTypeTreeItemResponseModel
> {
	constructor(host: UmbControllerHost) {
		super(host, {
			getRootItems: (args) =>
				DataTypeService.getTreeDataTypeRoot({
					query: {
						foldersOnly: args.foldersOnly,
						skip: args.paging.skip,
						take: args.paging.take,
					},
				}),

			getChildrenOf: (args) =>
				DataTypeService.getTreeDataTypeChildren({
					query: {
						parentId: args.parent.unique,
						foldersOnly: args.foldersOnly,
						skip: args.paging.skip,
						take: args.paging.take,
					},
				}),

			getAncestorsOf: (args) =>
				DataTypeService.getTreeDataTypeAncestors({
					query: {
						descendantId: args.treeItem.unique,
					},
				}),

			getSiblingsFrom: (args) =>
				DataTypeService.getTreeDataTypeSiblings({
					query: {
						target: args.paging.target.unique,
						before: args.paging.takeBefore,
						after: args.paging.takeAfter,
					},
				}),
		});
	}
}
