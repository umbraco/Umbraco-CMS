import type { UmbTreeItemModelBase } from '../types.js';
import type { UmbPagedModel, DataSourceResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbTreeDataSourceConstructor<TreeItemType extends UmbTreeItemModelBase> {
	new (host: UmbControllerHost): UmbTreeDataSource<TreeItemType>;
}

export interface UmbTreeDataSource<TreeItemType extends UmbTreeItemModelBase> {
	getRootItems(): Promise<DataSourceResponse<UmbPagedModel<TreeItemType>>>;
	getChildrenOf(parentUnique: string | null): Promise<DataSourceResponse<UmbPagedModel<TreeItemType>>>;
}
