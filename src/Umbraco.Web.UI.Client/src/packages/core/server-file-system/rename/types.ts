import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbRenameServerFileRepository<DetailType extends { unique: string }> {
	rename(unique: string, name: string): Promise<UmbDataSourceResponse<DetailType>>;
}

export interface UmbRenameServerFileDataSourceConstructor<DetailType extends { unique: string }> {
	new (host: UmbControllerHost): UmbRenameServerFileDataSource<DetailType>;
}

export interface UmbRenameServerFileDataSource<DetailType extends { unique: string }> {
	rename(unique: string, name: string): Promise<UmbDataSourceResponse<DetailType>>;
}
