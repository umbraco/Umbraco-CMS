import type { DataSourceResponse } from '../data-source-response.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbDetailDataSourceConstructor<DetailType = any> {
	new (host: UmbControllerHost): UmbDetailDataSource<DetailType>;
}

export interface UmbDetailDataSource<DetailType> {
	createScaffold(parentUnique: string | null, preset?: Partial<DetailType>): Promise<DataSourceResponse<DetailType>>;
	create(data: DetailType): Promise<DataSourceResponse<DetailType>>;
	read(unique: string): Promise<DataSourceResponse<DetailType>>;
	update(data: DetailType): Promise<DataSourceResponse<DetailType>>;
	delete(unique: string): Promise<DataSourceResponse>;
}
