import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbRenameServerFileRepository<DetailType extends UmbEntityModel> {
	rename(unique: string, name: string): Promise<UmbDataSourceResponse<DetailType>>;
}

export interface UmbRenameServerFileDataSourceConstructor<DetailType extends UmbEntityModel> {
	new (host: UmbControllerHost): UmbRenameServerFileDataSource<DetailType>;
}

export interface UmbRenameServerFileDataSource<DetailType extends UmbEntityModel> {
	rename(unique: string, name: string): Promise<UmbDataSourceResponse<DetailType>>;
}
