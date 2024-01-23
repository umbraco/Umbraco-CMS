import type { DataSourceResponse, UmbDataSourceErrorResponse } from '../data-source/index.js';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbDetailRepository<DetailModelType> {
	createScaffold(
		parentUnique: string | null,
		preset?: Partial<DetailModelType>,
	): Promise<DataSourceResponse<DetailModelType>>;
	requestByUnique(unique: string): Promise<DataSourceResponse<DetailModelType>>;
	byUnique(unique: string): Promise<Observable<DetailModelType | undefined>>;
	create(data: DetailModelType): Promise<DataSourceResponse<DetailModelType>>;
	save(data: DetailModelType): Promise<DataSourceResponse<DetailModelType>>;
	delete(unique: string): Promise<UmbDataSourceErrorResponse>;
}
