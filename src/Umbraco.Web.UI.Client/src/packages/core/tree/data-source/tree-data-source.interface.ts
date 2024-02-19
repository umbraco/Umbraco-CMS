import type { UmbTreeItemModelBase } from '../types.js';
import type { UmbPagedModel, DataSourceResponse } from '@umbraco-cms/backoffice/repository';
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
	 * @return {*}  {Promise<DataSourceResponse<UmbPagedModel<TreeItemType>>>}
	 * @memberof UmbTreeDataSource
	 */
	getRootItems(): Promise<DataSourceResponse<UmbPagedModel<TreeItemType>>>;

	/**
	 * Gets the children of the given parent item.
	 * @param {(string | null)} parentUnique
	 * @return {*}  {Promise<DataSourceResponse<UmbPagedModel<TreeItemType>>}
	 * @memberof UmbTreeDataSource
	 */
	getChildrenOf(parentUnique: string | null): Promise<DataSourceResponse<UmbPagedModel<TreeItemType>>>;
}
