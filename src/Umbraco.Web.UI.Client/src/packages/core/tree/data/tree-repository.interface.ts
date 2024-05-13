import type { UmbUniqueTreeItemModel, UmbUniqueTreeRootModel } from '../types.js';
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
 * @export
 * @interface UmbTreeRepository
 * @extends {UmbApi}
 * @template TreeItemType
 * @template TreeRootType
 */
export interface UmbTreeRepository<
	TreeItemType extends UmbUniqueTreeItemModel = UmbUniqueTreeItemModel,
	TreeRootType extends UmbUniqueTreeRootModel = UmbUniqueTreeRootModel,
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
	requestRootTreeItems: (args: UmbTreeRootItemsRequestArgs) => Promise<{
		data?: UmbPagedModel<TreeItemType>;
		error?: ProblemDetails;
		asObservable?: () => Observable<TreeItemType[]>;
	}>;

	/**
	 * Requests the children of the given parent item.
	 * @param {UmbTreeChildrenOfRequestArgs} args
	 * @memberof UmbTreeRepository
	 */
	requestTreeItemsOf: (args: UmbTreeChildrenOfRequestArgs) => Promise<{
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
		args: UmbTreeAncestorsOfRequestArgs,
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
