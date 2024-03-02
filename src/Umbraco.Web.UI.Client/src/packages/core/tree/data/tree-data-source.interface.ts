import type { UmbTreeItemModelBase } from '../types.js';
import type { UmbTreeChildrenOfRequestArgs, UmbTreeRootItemsRequestArgs } from './types.js';
import type { UmbPagedModel, UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * Interface for a tree data source constructor.
 * @export
 * @interface UmbTreeDataSourceConstructor
 * @template TreeItemType
 */
export interface UmbTreeDataSourceConstructor<TreeItemType extends UmbTreeItemModelBase> {
	new (host: UmbControllerHost): UmbTreeDataSource<TreeItemType>;
}

/**
 * Interface for a tree data source.
 * @export
 * @interface UmbTreeDataSource
 * @template TreeItemType
 */
export interface UmbTreeDataSource<TreeItemType extends UmbTreeItemModelBase> {
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
}
