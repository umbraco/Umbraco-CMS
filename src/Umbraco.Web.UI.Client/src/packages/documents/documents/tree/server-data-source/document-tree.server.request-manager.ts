/* eslint-disable local-rules/no-direct-api-import */

import type {
	UmbDocumentTreeChildrenOfRequestArgs,
	UmbDocumentTreeItemsRequestArgs,
	UmbDocumentTreeRootItemsRequestArgs,
} from '../types.js';
import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import {
	UmbManagementApiInFlightRequestCache,
	UmbManagementApiTreeDataRequestManager,
} from '@umbraco-cms/backoffice/management-api';
import type {
	BatchResponseModelDocumentTreeItemResponseModel,
	DocumentTreeItemResponseModel,
	PagedDocumentTreeItemResponseModel,
	SubsetDocumentTreeItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbManagementApiTreeAncestorsOfRequestArgs,
	UmbManagementApiTreeBatchRequestArgs,
	UmbManagementApiTreeChildrenOfRequestArgs,
	UmbManagementApiTreeRootItemsRequestArgs,
	UmbManagementApiTreeSiblingsFromRequestArgs,
} from '@umbraco-cms/backoffice/management-api';

interface UmbManagementApiDocumentTreeRootItemsRequestArgs
	extends UmbManagementApiTreeRootItemsRequestArgs, Pick<UmbDocumentTreeRootItemsRequestArgs, 'dataType'> {}

interface UmbManagementApiDocumentTreeChildrenOfRequestArgs
	extends UmbManagementApiTreeChildrenOfRequestArgs, Pick<UmbDocumentTreeChildrenOfRequestArgs, 'dataType'> {}

interface UmbManagementApiDocumentTreeSiblingsFromRequestArgs extends UmbManagementApiTreeSiblingsFromRequestArgs {
	dataType?: {
		unique: string;
	};
}

interface UmbManagementApiDocumentTreeBatchRequestArgs
	extends UmbManagementApiTreeBatchRequestArgs, Pick<UmbDocumentTreeItemsRequestArgs, 'dataType'> {}

export class UmbManagementApiDocumentTreeDataRequestManager extends UmbManagementApiTreeDataRequestManager<
	DocumentTreeItemResponseModel,
	UmbManagementApiDocumentTreeRootItemsRequestArgs,
	PagedDocumentTreeItemResponseModel,
	UmbManagementApiDocumentTreeChildrenOfRequestArgs,
	PagedDocumentTreeItemResponseModel,
	UmbManagementApiTreeAncestorsOfRequestArgs,
	Array<DocumentTreeItemResponseModel>,
	UmbManagementApiDocumentTreeSiblingsFromRequestArgs,
	SubsetDocumentTreeItemResponseModel,
	UmbManagementApiDocumentTreeBatchRequestArgs,
	BatchResponseModelDocumentTreeItemResponseModel
> {
	static #inflightRequestCache = new UmbManagementApiInFlightRequestCache<unknown>();

	constructor(host: UmbControllerHost) {
		super(host, {
			getRootItems: (args) =>
				DocumentService.getTreeDocumentRoot({
					query: {
						dataTypeId: args.dataType?.unique,
						skip: args.paging.skip,
						take: args.paging.take,
					},
				}),

			getChildrenOf: (args) =>
				DocumentService.getTreeDocumentChildren({
					query: {
						parentId: args.parent.unique,
						dataTypeId: args.dataType?.unique,
						skip: args.paging.skip,
						take: args.paging.take,
					},
				}),

			getAncestorsOf: (args) =>
				DocumentService.getTreeDocumentAncestors({
					query: {
						descendantId: args.treeItem.unique,
					},
				}),

			getSiblingsFrom: (args) =>
				DocumentService.getTreeDocumentSiblings({
					query: {
						dataTypeId: args.dataType?.unique,
						target: args.paging.target.unique,
						before: args.paging.takeBefore,
						after: args.paging.takeAfter,
					},
				}),

			getBatch: (args) =>
				DocumentService.getTreeDocumentBatch({
					query: {
						id: args.uniques,
						dataTypeId: args.dataType?.unique,
					},
				}),

			inflightRequestCache: UmbManagementApiDocumentTreeDataRequestManager.#inflightRequestCache,
		});
	}
}
