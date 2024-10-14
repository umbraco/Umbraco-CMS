import type { UmbTreeItemModel, UmbTreeRootModel } from '../types.js';
import type {
	UmbTreeChildrenOfRequestArgs,
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeRootItemsRequestArgs,
} from './types.js';
import type { UmbPagedModel } from '@umbraco-cms/backoffice/repository';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { ProblemDetails } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

/**
 * Interface for a tree repository.
 * @interface UmbTreeRepository
 * @augments {UmbApi}
 * @template TreeItemType
 * @template TreeRootType
 */
export interface UmbTreeRepository<
	TreeItemType extends UmbTreeItemModel = UmbTreeItemModel,
	TreeRootType extends UmbTreeRootModel = UmbTreeRootModel,
	TreeRootItemsRequestArgsType extends UmbTreeRootItemsRequestArgs = UmbTreeRootItemsRequestArgs,
	TreeChildrenOfRequestArgsType extends UmbTreeChildrenOfRequestArgs = UmbTreeChildrenOfRequestArgs,
	TreeAncestorsOfRequestArgsType extends UmbTreeAncestorsOfRequestArgs = UmbTreeAncestorsOfRequestArgs,
> extends UmbApi {
	/**
	 * Requests the root of the tree.
	 * @memberof UmbTreeRepository
	 */
	requestTreeRoot: () => Promise<{
		data?: TreeRootType;
		error?: ProblemDetails;
	}>;

	/**
	 * Requests the root items of the tree.
	 * @param {UmbTreeRootItemsRequestArgs} args
	 * @memberof UmbTreeRepository
	 */
	requestTreeRootItems: (args: TreeRootItemsRequestArgsType) => Promise<{
		data?: UmbPagedModel<TreeItemType>;
		error?: ProblemDetails;
		asObservable?: () => Observable<TreeItemType[]>;
	}>;

	/**
	 * Requests the children of the given parent item.
	 * @param {UmbTreeChildrenOfRequestArgs} args
	 * @memberof UmbTreeRepository
	 */
	requestTreeItemsOf: (args: TreeChildrenOfRequestArgsType) => Promise<{
		data?: UmbPagedModel<TreeItemType>;
		error?: ProblemDetails;
		asObservable?: () => Observable<TreeItemType[]>;
	}>;

	/**
	 * Requests the ancestors of the given item.
	 * @param {UmbTreeAncestorsOfRequestArgs} args
	 * @memberof UmbTreeRepository
	 */
	requestTreeItemAncestors: (
		args: TreeAncestorsOfRequestArgsType,
	) => Promise<{ data?: TreeItemType[]; error?: ProblemDetails; asObservable?: () => Observable<TreeItemType[]> }>;

	/**
	 * Returns an observable of the root items of the tree.
	 * @memberof UmbTreeRepository
	 */
	rootTreeItems: () => Promise<Observable<TreeItemType[]>>;

	/**
	 * Returns an observable of the children of the given parent item.
	 * @param {(string | null)} parentUnique
	 * @memberof UmbTreeRepository
	 */
	treeItemsOf: (parentUnique: string | null) => Promise<Observable<TreeItemType[]>>;
}
