import type { DataSourceResponse, UmbDataSourceErrorResponse } from './data-source/index.js';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbDetailRepository<
	CreateRequestType = any,
	CreateResponseType = any,
	UpdateRequestType = any,
	ResponseType = any,
	CreateScaffoldPresetType = Partial<CreateRequestType>
> {
	createScaffold(
		parentId: string | null,
		preset?: Partial<CreateRequestType> | CreateScaffoldPresetType
	): Promise<DataSourceResponse<CreateRequestType>>;
	requestById(id: string): Promise<DataSourceResponse<ResponseType | undefined>>;
	byId(id: string): Promise<Observable<ResponseType | undefined>>;
	create(data: CreateRequestType): Promise<DataSourceResponse<CreateResponseType>>;
	save(id: string, data: UpdateRequestType): Promise<UmbDataSourceErrorResponse>;
	delete(id: string): Promise<UmbDataSourceErrorResponse>;
}
