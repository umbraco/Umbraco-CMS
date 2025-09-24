/* eslint-disable local-rules/no-direct-api-import */

import type { UmbMediaTreeChildrenOfRequestArgs, UmbMediaTreeRootItemsRequestArgs } from '../types.js';
import { MediaService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiTreeDataRequestManager } from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	MediaTreeItemResponseModel,
	PagedMediaTreeItemResponseModel,
	SubsetMediaTreeItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import type {
	UmbManagementApiTreeAncestorsOfRequestArgs,
	UmbManagementApiTreeChildrenOfRequestArgs,
	UmbManagementApiTreeRootItemsRequestArgs,
	UmbManagementApiTreeSiblingsFromRequestArgs,
} from '@umbraco-cms/backoffice/management-api';

interface UmbManagementApiMediaTreeRootItemsRequestArgs
	extends UmbManagementApiTreeRootItemsRequestArgs,
		Pick<UmbMediaTreeRootItemsRequestArgs, 'dataType'> {}

interface UmbManagementApiMediaTreeChildrenOfRequestArgs
	extends UmbManagementApiTreeChildrenOfRequestArgs,
		Pick<UmbMediaTreeChildrenOfRequestArgs, 'dataType'> {}

interface UmbManagementApiMediaTreeSiblingsFromRequestArgs extends UmbManagementApiTreeSiblingsFromRequestArgs {
	dataType?: {
		unique: string;
	};
}

export class UmbManagementApiMediaTreeDataRequestManager extends UmbManagementApiTreeDataRequestManager<
	MediaTreeItemResponseModel,
	UmbManagementApiMediaTreeRootItemsRequestArgs,
	PagedMediaTreeItemResponseModel,
	UmbManagementApiMediaTreeChildrenOfRequestArgs,
	PagedMediaTreeItemResponseModel,
	UmbManagementApiTreeAncestorsOfRequestArgs,
	Array<MediaTreeItemResponseModel>,
	UmbManagementApiMediaTreeSiblingsFromRequestArgs,
	SubsetMediaTreeItemResponseModel
> {
	constructor(host: UmbControllerHost) {
		super(host, {
			getRootItems: (args) =>
				MediaService.getTreeMediaRoot({
					query: {
						dataTypeId: args.dataType?.unique,
						skip: args.paging.skip,
						take: args.paging.take,
					},
				}),

			getChildrenOf: (args) =>
				MediaService.getTreeMediaChildren({
					query: {
						parentId: args.parent.unique,
						dataTypeId: args.dataType?.unique,
						skip: args.paging.skip,
						take: args.paging.take,
					},
				}),

			getAncestorsOf: (args) =>
				MediaService.getTreeMediaAncestors({
					query: {
						descendantId: args.treeItem.unique,
					},
				}),

			getSiblingsFrom: (args) =>
				MediaService.getTreeMediaSiblings({
					query: {
						dataTypeId: args.dataType?.unique,
						target: args.paging.target.unique,
						before: args.paging.takeBefore,
						after: args.paging.takeAfter,
					},
				}),
		});
	}
}
