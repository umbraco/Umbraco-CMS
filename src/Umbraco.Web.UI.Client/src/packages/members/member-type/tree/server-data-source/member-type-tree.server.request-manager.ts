/* eslint-disable local-rules/no-direct-api-import */

import { MemberTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiTreeDataRequestManager } from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	MemberTypeTreeItemResponseModel,
	PagedMemberTypeTreeItemResponseModel,
	SubsetMemberTypeTreeItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import type {
	UmbManagementApiTreeAncestorsOfRequestArgs,
	UmbManagementApiTreeChildrenOfRequestArgs,
	UmbManagementApiTreeRootItemsRequestArgs,
	UmbManagementApiTreeSiblingsFromRequestArgs,
} from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiMemberTypeTreeDataRequestManager extends UmbManagementApiTreeDataRequestManager<
	MemberTypeTreeItemResponseModel,
	UmbManagementApiTreeRootItemsRequestArgs,
	PagedMemberTypeTreeItemResponseModel,
	UmbManagementApiTreeChildrenOfRequestArgs,
	PagedMemberTypeTreeItemResponseModel,
	UmbManagementApiTreeAncestorsOfRequestArgs,
	Array<MemberTypeTreeItemResponseModel>,
	UmbManagementApiTreeSiblingsFromRequestArgs,
	SubsetMemberTypeTreeItemResponseModel
> {
	constructor(host: UmbControllerHost) {
		super(host, {
			getRootItems: (args) =>
				MemberTypeService.getTreeMemberTypeRoot({
					query: {
						foldersOnly: args.foldersOnly,
						skip: args.paging.skip,
						take: args.paging.take,
					},
				}),

			getChildrenOf: (args) =>
				MemberTypeService.getTreeMemberTypeChildren({
					query: {
						parentId: args.parent.unique,
						foldersOnly: args.foldersOnly,
						skip: args.paging.skip,
						take: args.paging.take,
					},
				}),

			getAncestorsOf: (args) =>
				MemberTypeService.getTreeMemberTypeAncestors({
					query: {
						descendantId: args.treeItem.unique,
					},
				}),

			getSiblingsFrom: (args) =>
				MemberTypeService.getTreeMemberTypeSiblings({
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
