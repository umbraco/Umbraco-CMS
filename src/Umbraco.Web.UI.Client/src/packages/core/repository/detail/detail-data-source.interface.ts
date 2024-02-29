import type { UmbDataSourceResponse } from '../data-source-response.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbDetailDataSourceConstructor<DetailType = any> {
	new (host: UmbControllerHost): UmbDetailDataSource<DetailType>;
}

export interface UmbDetailDataSource<DetailType> {
	createScaffold(preset?: Partial<DetailType>): Promise<UmbDataSourceResponse<DetailType>>;
	create(data: DetailType): Promise<UmbDataSourceResponse<DetailType>>;
	read(unique: string): Promise<UmbDataSourceResponse<DetailType>>;
	update(data: DetailType): Promise<UmbDataSourceResponse<DetailType>>;
	delete(unique: string): Promise<UmbDataSourceResponse>;
}
