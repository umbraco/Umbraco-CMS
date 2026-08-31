import type { UmbTreeItemModel, UmbTreeRootModel } from '../types.js';
import type {
	UmbCreateTreeItemDataResolverArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeItemDataResolver,
	UmbTreeRootItemsRequestArgs,
} from './types.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type {
	UmbRepositoryResponse,
	UmbRepositoryResponseWithAsObservable,
	UmbTargetPagedModel,
} from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

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
	 * Creates an item data resolver for a tree item. Implement this to resolve names and icons that cannot be
	 * read directly off the tree item, such as variant aware document names.
	 * @param {UmbControllerHost} host - The controller host of the consumer.
	 * @param {UmbCreateTreeItemDataResolverArgs} [args] - Optional arguments for selecting the resolver.
	 * @returns {UmbTreeItemDataResolver | undefined} A resolver for the item, if one is available.
	 * @memberof UmbTreeRepository
	 */
	createTreeItemDataResolver?: (
		host: UmbControllerHost,
		args?: UmbCreateTreeItemDataResolverArgs,
	) => UmbTreeItemDataResolver<TreeItemType> | undefined;

	/**
	 * Requests the root items of the tree.
	 * @param {UmbTreeRootItemsRequestArgs} args
	 * @memberof UmbTreeRepository
	 */
	requestTreeRootItems: (
		args: TreeRootItemsRequestArgsType,
	) => Promise<UmbRepositoryResponseWithAsObservable<UmbTargetPagedModel<TreeItemType>, TreeItemType[]>>;

	/**
	 * Requests the children of the given parent item.
	 * @param {UmbTreeChildrenOfRequestArgs} args
	 * @memberof UmbTreeRepository
	 */
	requestTreeItemsOf: (
		args: TreeChildrenOfRequestArgsType,
	) => Promise<UmbRepositoryResponseWithAsObservable<UmbTargetPagedModel<TreeItemType>, TreeItemType[]>>;

	/**
	 * Requests the ancestors of the given item.
	 * @param {UmbTreeAncestorsOfRequestArgs} args
	 * @memberof UmbTreeRepository
	 */
	requestTreeItemAncestors: (args: TreeAncestorsOfRequestArgsType) => Promise<UmbRepositoryResponse<TreeItemType[]>>;
}
