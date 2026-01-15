import type { UmbOffsetPaginationRequestModel, UmbTargetPaginationRequestModel } from '@umbraco-cms/backoffice/utils';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeRootItemsRequestArgs,
} from '@umbraco-cms/backoffice/tree';

export interface UmbManagementApiTreeRootItemsRequestArgs extends UmbTreeRootItemsRequestArgs {
	paging: UmbOffsetPaginationRequestModel;
}

export interface UmbManagementApiTreeChildrenOfRequestArgs extends UmbTreeChildrenOfRequestArgs {
	parent: {
		unique: string;
		entityType: string;
	};
	paging: UmbOffsetPaginationRequestModel;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbManagementApiTreeAncestorsOfRequestArgs extends UmbTreeAncestorsOfRequestArgs {}

export interface UmbManagementApiTreeSiblingsFromRequestArgs extends UmbTreeRootItemsRequestArgs {
	paging: {
		target: {
			entityType: string;
			unique: string;
		};
		takeBefore: UmbTargetPaginationRequestModel['takeBefore'];
		takeAfter: UmbTargetPaginationRequestModel['takeAfter'];
	};
}
