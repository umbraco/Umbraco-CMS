import type { DataSourceResponse } from '../data-source/index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbDetailDataSourceConstructor<DetailType = any> {
	new (host: UmbControllerHost): UmbDetailDataSource<DetailType>;
}

export interface UmbDetailDataSource<DetailType> {
	createScaffold(preset?: Partial<DetailType>): Promise<DataSourceResponse<DetailType>>;
	create(model: DetailType, parentUnique: string | null): Promise<DataSourceResponse<DetailType>>;
	read(unique: string): Promise<DataSourceResponse<DetailType>>;
	update(model: DetailType): Promise<DataSourceResponse<DetailType>>;
	delete(unique: string): Promise<DataSourceResponse>;
}
