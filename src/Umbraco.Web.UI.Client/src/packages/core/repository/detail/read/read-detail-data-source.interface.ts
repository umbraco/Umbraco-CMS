import type { UmbDataSourceResponse } from '../../data-source-response.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbReadDetailDataSourceConstructor<DetailType = any> {
	new (host: UmbControllerHost): UmbReadDetailDataSource<DetailType>;
}

export interface UmbReadDetailDataSource<DetailType> {
	read(unique: string): Promise<UmbDataSourceResponse<DetailType>>;
}
