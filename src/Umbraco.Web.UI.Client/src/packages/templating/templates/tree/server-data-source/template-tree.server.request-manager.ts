/* eslint-disable local-rules/no-direct-api-import */

import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	TemplateService,
	type NamedEntityTreeItemResponseModel,
	type PagedNamedEntityTreeItemResponseModel,
	type SubsetNamedEntityTreeItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import {
	UmbManagementApiTreeDataRequestManager,
	type UmbManagementApiTreeAncestorsOfRequestArgs,
	type UmbManagementApiTreeChildrenOfRequestArgs,
	type UmbManagementApiTreeRootItemsRequestArgs,
	type UmbManagementApiTreeSiblingsFromRequestArgs,
} from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiTemplateTreeDataRequestManager extends UmbManagementApiTreeDataRequestManager<
	NamedEntityTreeItemResponseModel,
	UmbManagementApiTreeRootItemsRequestArgs,
	PagedNamedEntityTreeItemResponseModel,
	UmbManagementApiTreeChildrenOfRequestArgs,
	PagedNamedEntityTreeItemResponseModel,
	UmbManagementApiTreeAncestorsOfRequestArgs,
	Array<NamedEntityTreeItemResponseModel>,
	UmbManagementApiTreeSiblingsFromRequestArgs,
	SubsetNamedEntityTreeItemResponseModel
> {
	constructor(host: UmbControllerHost) {
		super(host, {
			getRootItems: (args) =>
				TemplateService.getTreeTemplateRoot({
					query: {
						skip: args.paging.skip,
						take: args.paging.take,
					},
				}),

			getChildrenOf: (args) =>
				TemplateService.getTreeTemplateChildren({
					query: {
						parentId: args.parent.unique,
						skip: args.paging.skip,
						take: args.paging.take,
					},
				}),

			getAncestorsOf: (args) =>
				TemplateService.getTreeTemplateAncestors({
					query: {
						descendantId: args.treeItem.unique,
					},
				}),

			getSiblingsFrom: (args) =>
				TemplateService.getTreeTemplateSiblings({
					query: {
						target: args.paging.target.unique,
						before: args.paging.takeBefore,
						after: args.paging.takeAfter,
					},
				}),
		});
	}
}
