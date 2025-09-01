/* eslint-disable local-rules/no-direct-api-import */

import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	DocumentTypeService,
	type DocumentTypeTreeItemResponseModel,
	type PagedDocumentTypeTreeItemResponseModel,
	type SubsetDocumentTypeTreeItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiTreeDataRequestManager } from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiDocumentTypeTreeDataRequestManager extends UmbManagementApiTreeDataRequestManager<
	DocumentTypeTreeItemResponseModel,
	PagedDocumentTypeTreeItemResponseModel,
	PagedDocumentTypeTreeItemResponseModel,
	Array<DocumentTypeTreeItemResponseModel>,
	SubsetDocumentTypeTreeItemResponseModel
> {
	constructor(host: UmbControllerHost) {
		super(host, {
			getRootItems: (args: any) =>
				DocumentTypeService.getTreeDocumentTypeRoot({
					query: {
						foldersOnly: args.foldersOnly,
						skip: args.skip,
						take: args.take,
					},
				}),

			getChildrenOf: (args: any) =>
				DocumentTypeService.getTreeDocumentTypeChildren({
					query: {
						parentId: args.parent.unique,
						foldersOnly: args.foldersOnly,
						skip: args.skip,
						take: args.take,
					},
				}),

			getAncestorsOf: (args: any) =>
				DocumentTypeService.getTreeDocumentTypeAncestors({
					query: {
						descendantId: args.treeItem.unique,
					},
				}),

			getSiblingsFrom: (args: any) =>
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
