/* eslint-disable local-rules/no-direct-api-import */

import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	DataTypeService,
	type DataTypeTreeItemResponseModel,
	type GetTreeDataTypeAncestorsData,
	type GetTreeDataTypeChildrenData,
	type GetTreeDataTypeRootData,
	type PagedDataTypeTreeItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiTreeDataRequestManager } from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiDataTypeTreeDataRequestManager extends UmbManagementApiTreeDataRequestManager<
	PagedDataTypeTreeItemResponseModel,
	PagedDataTypeTreeItemResponseModel,
	DataTypeTreeItemResponseModel
> {
	constructor(host: UmbControllerHost) {
		super(host, {
			getRootItems: (args: GetTreeDataTypeRootData['query']) =>
				DataTypeService.getTreeDataTypeRoot({
					query: { foldersOnly: args.foldersOnly, skip: args.skip, take: args.take },
				}),

			getChildrenOf: (args: GetTreeDataTypeChildrenData['query']) =>
				DataTypeService.getTreeDataTypeChildren({
					query: { parentId: args.parentId, foldersOnly: args.foldersOnly, skip: args.skip, take: args.take },
				}),

			getAncestorsOf: (args: GetTreeDataTypeAncestorsData['query']) =>
				DataTypeService.getTreeDataTypeAncestors({
					query: { descendantId: args.descendantId },
				}),
		});
	}
}
