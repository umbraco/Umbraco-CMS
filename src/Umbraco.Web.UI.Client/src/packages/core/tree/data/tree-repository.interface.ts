import type { UmbTreeItemModel, UmbTreeRootModel } from '../types.js';
import type {
	UmbTreeChildrenOfRequestArgs,
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeRootItemsRequestArgs,
} from './types.js';
import type {
	UmbPagedModel,
	UmbRepositoryResponse,
	UmbRepositoryResponseWithAsObservable,
} from '@umbraco-cms/backoffice/repository';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
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
	requestTreeRoot: () => Promise<UmbRepositoryResponse<TreeRootType>>;

	/**
	 * Requests the root items of the tree.
	 * @param {UmbTreeRootItemsRequestArgs} args
	 * @memberof UmbTreeRepository
	 */
	requestTreeRootItems: (
		args: TreeRootItemsRequestArgsType,
	) => Promise<UmbRepositoryResponseWithAsObservable<UmbPagedModel<TreeItemType>, TreeItemType[]>>;

	/**
	 * Requests the children of the given parent item.
	 * @param {UmbTreeChildrenOfRequestArgs} args
	 * @memberof UmbTreeRepository
	 */
	requestTreeItemsOf: (
		args: TreeChildrenOfRequestArgsType,
	) => Promise<UmbRepositoryResponseWithAsObservable<UmbPagedModel<TreeItemType>, TreeItemType[]>>;

	/**
	 * Requests the ancestors of the given item.
	 * @param {UmbTreeAncestorsOfRequestArgs} args
	 * @memberof UmbTreeRepository
	 */
	requestTreeItemAncestors: (args: TreeAncestorsOfRequestArgsType) => Promise<UmbRepositoryResponse<TreeItemType[]>>;

	/**
	 * Returns an observable of the root items of the tree.
	 * @memberof UmbTreeRepository
	 * @deprecated Use `requestTreeRootItems` instead. It will be removed in Umbraco 18.
	 */
	rootTreeItems: () => Promise<Observable<TreeItemType[]>>;

	/**
	 * Returns an observable of the children of the given parent item.
	 * @param {(string | null)} parentUnique
	 * @memberof UmbTreeRepository
	 * @deprecated Use `requestTreeItemsOf` instead. It will be removed in Umbraco 18.
	 */
	treeItemsOf: (parentUnique: string | null) => Promise<Observable<TreeItemType[]>>;
}
