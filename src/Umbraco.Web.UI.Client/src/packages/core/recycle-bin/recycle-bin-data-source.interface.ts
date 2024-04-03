import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbRecycleBinDataSourceConstructor {
	new (host: UmbControllerHost): UmbRecycleBinDataSource;
}

export interface UmbRecycleBinDataSource {
	trash(args: any): Promise<UmbDataSourceResponse>;
	restore(args: any): Promise<UmbDataSourceResponse>;
	empty(): Promise<UmbDataSourceResponse>;
	getOriginalParent(args: any): Promise<UmbDataSourceResponse>;
}
