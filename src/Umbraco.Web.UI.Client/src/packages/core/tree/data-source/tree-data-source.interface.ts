import { UmbTreeItemModelBase } from '../types.js';
import type { UmbPagedData, DataSourceResponse } from '@umbraco-cms/backoffice/repository';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbTreeDataSourceConstructor<
	TreeItemType extends UmbTreeItemModelBase,
	TreeRootType extends UmbTreeItemModelBase,
> {
	new (host: UmbControllerHost): UmbTreeDataSource<TreeItemType, TreeRootType>;
}

export interface UmbTreeDataSource<
	TreeItemType extends UmbTreeItemModelBase,
	TreeRootType extends UmbTreeItemModelBase,
> {
	getTreeRoot?(): Promise<DataSourceResponse<TreeRootType>>;
	getRootItems(): Promise<DataSourceResponse<UmbPagedData<TreeItemType>>>;
	getChildrenOf(parentUnique: string | null): Promise<DataSourceResponse<UmbPagedData<TreeItemType>>>;
	// TODO: remove this when all repositories are migrated to the new items interface
	getItems(unique: Array<string>): Promise<DataSourceResponse<Array<any>>>;
}
