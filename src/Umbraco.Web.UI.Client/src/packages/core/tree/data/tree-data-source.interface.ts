import type { UmbUniqueTreeItemModel } from '../types.js';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeRootItemsRequestArgs,
} from './types.js';
import type { UmbPagedModel, UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * Interface for a tree data source constructor.
 * @export
 * @interface UmbTreeDataSourceConstructor
 * @template TreeItemType
 */
export interface UmbTreeDataSourceConstructor<TreeItemType extends UmbUniqueTreeItemModel> {
	new (host: UmbControllerHost): UmbTreeDataSource<TreeItemType>;
}

/**
 * Interface for a tree data source.
 * @export
 * @interface UmbTreeDataSource
 * @template TreeItemType
 */
export interface UmbTreeDataSource<TreeItemType extends UmbUniqueTreeItemModel> {
	/**
	 * Gets the root items of the tree.
	 * @return {*}  {Promise<UmbDataSourceResponse<UmbPagedModel<TreeItemType>>>}
	 * @memberof UmbTreeDataSource
	 */
	getRootItems(args: UmbTreeRootItemsRequestArgs): Promise<UmbDataSourceResponse<UmbPagedModel<TreeItemType>>>;

	/**
	 * Gets the children of the given parent item.
	 * @return {*}  {Promise<UmbDataSourceResponse<UmbPagedModel<TreeItemType>>}
	 * @memberof UmbTreeDataSource
	 */
	getChildrenOf(args: UmbTreeChildrenOfRequestArgs): Promise<UmbDataSourceResponse<UmbPagedModel<TreeItemType>>>;

	/**
	 * Gets the ancestors of the given item.
	 * @return {*}  {Promise<UmbDataSourceResponse<Array<TreeItemType>>}
	 * @memberof UmbTreeDataSource
	 */
	getAncestorsOf(args: UmbTreeAncestorsOfRequestArgs): Promise<UmbDataSourceResponse<Array<TreeItemType>>>;
}
