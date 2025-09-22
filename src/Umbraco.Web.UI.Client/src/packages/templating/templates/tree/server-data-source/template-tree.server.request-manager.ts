/* eslint-disable local-rules/no-direct-api-import */

import { TemplateService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiTreeDataRequestManager } from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	NamedEntityTreeItemResponseModel,
	PagedNamedEntityTreeItemResponseModel,
	SubsetNamedEntityTreeItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import type {
	UmbManagementApiTreeAncestorsOfRequestArgs,
	UmbManagementApiTreeChildrenOfRequestArgs,
	UmbManagementApiTreeRootItemsRequestArgs,
	UmbManagementApiTreeSiblingsFromRequestArgs,
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
