import type { DataSourceResponse, UmbDataSourceErrorResponse } from '../data-source-response.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbDetailRepository<DetailModelType> {
	createScaffold(preset?: Partial<DetailModelType>): Promise<DataSourceResponse<DetailModelType>>;
	requestByUnique(unique: string): Promise<DataSourceResponse<DetailModelType>>;
	byUnique(unique: string): Promise<Observable<DetailModelType | undefined>>;
	create(model: DetailModelType, parentUnique: string | null): Promise<DataSourceResponse<DetailModelType>>;
	save(model: DetailModelType): Promise<DataSourceResponse<DetailModelType>>;
	delete(unique: string): Promise<UmbDataSourceErrorResponse>;
}
