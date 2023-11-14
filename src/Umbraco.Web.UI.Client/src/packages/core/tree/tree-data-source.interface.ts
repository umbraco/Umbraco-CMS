import type { UmbPagedData } from '../repository/data-source/types.js';
import type { DataSourceResponse } from '../repository/data-source/data-source-response.interface.js';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbTreeDataSourceConstructor<TreeItemType = any, TreeRootType = any> {
	new (host: UmbControllerHost): UmbTreeDataSource<TreeItemType, TreeRootType>;
}

export interface UmbTreeDataSource<TreeItemType = any, TreeRootType = any> {
	getTreeRoot?(): Promise<DataSourceResponse<TreeRootType>>;
	getRootItems(): Promise<DataSourceResponse<UmbPagedData<TreeItemType>>>;
	getChildrenOf(parentUnique: string | null): Promise<DataSourceResponse<UmbPagedData<TreeItemType>>>;
	// TODO: remove this when all repositories are migrated to the new items interface
	getItems(unique: Array<string>): Promise<DataSourceResponse<Array<any>>>;
}
