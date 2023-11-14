import type { UmbPagedData } from './types.js';
import type { DataSourceResponse } from './data-source-response.interface.js';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbEntityTreeRootModel, UmbTreeItemModel, UmbTreeItemModelBase } from '@umbraco-cms/backoffice/tree';

export interface UmbTreeDataSourceConstructor<TreeItemType extends UmbTreeItemModel = UmbTreeItemModel> {
	new (host: UmbControllerHost): UmbTreeDataSource<TreeItemType>;
}

export interface UmbTreeDataSource<
	TreeItemType extends UmbTreeItemModel = UmbTreeItemModel,
	TreeRootType extends UmbTreeItemModelBase = UmbEntityTreeRootModel,
> {
	getTreeRoot?(): Promise<DataSourceResponse<TreeRootType>>;
	getRootItems(): Promise<DataSourceResponse<UmbPagedData<TreeItemType>>>;
	getChildrenOf(parentUnique: string | null): Promise<DataSourceResponse<UmbPagedData<TreeItemType>>>;
	// TODO: remove this when all repositories are migrated to the new items interface
	getItems(unique: Array<string>): Promise<DataSourceResponse<Array<any>>>;
}
