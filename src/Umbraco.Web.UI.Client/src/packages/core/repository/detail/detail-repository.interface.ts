import type { UmbDataSourceResponse, UmbDataSourceErrorResponse } from '../data-source-response.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbDetailRepository<DetailModelType> {
	createScaffold(
		parentUnique: string | null,
		preset?: Partial<DetailModelType>,
	): Promise<UmbDataSourceResponse<DetailModelType>>;
	requestByUnique(unique: string): Promise<UmbDataSourceResponse<DetailModelType>>;
	byUnique(unique: string): Promise<Observable<DetailModelType | undefined>>;
	create(data: DetailModelType): Promise<UmbDataSourceResponse<DetailModelType>>;
	save(data: DetailModelType): Promise<UmbDataSourceResponse<DetailModelType>>;
	delete(unique: string): Promise<UmbDataSourceErrorResponse>;
}
