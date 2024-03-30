import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbRenameRepository<DetailType extends { unique: string }> {
	rename(unique: string, name: string): Promise<UmbDataSourceResponse<DetailType>>;
}

export interface UmbRenameDataSourceConstructor<DetailType extends { unique: string }> {
	new (host: UmbControllerHost): UmbRenameDataSource<DetailType>;
}

export interface UmbRenameDataSource<DetailType extends { unique: string }> {
	rename(unique: string, name: string): Promise<UmbDataSourceResponse<DetailType>>;
}
