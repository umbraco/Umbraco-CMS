import type { UmbDataSourceResponse } from '../data-source-response.interface.js';
import type { UmbReadDetailDataSource } from './read/index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbDetailDataSourceConstructor<
	DetailType = any,
	UmbDetailDataSourceType extends UmbDetailDataSource<DetailType> = UmbDetailDataSource<DetailType>,
> {
	new (host: UmbControllerHost): UmbDetailDataSourceType;
}

export interface UmbDetailDataSource<DetailType> extends UmbReadDetailDataSource<DetailType> {
	createScaffold(preset?: Partial<DetailType>): Promise<UmbDataSourceResponse<DetailType>>;
	create(data: DetailType, parentUnique: string | null): Promise<UmbDataSourceResponse<DetailType>>;
	update(data: DetailType): Promise<UmbDataSourceResponse<DetailType>>;
	delete(unique: string): Promise<UmbDataSourceResponse<unknown>>;
}
