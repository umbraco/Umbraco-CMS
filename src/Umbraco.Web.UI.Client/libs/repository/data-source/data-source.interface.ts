import type { DataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbDataSource<CreateRequestType, CreateResponseType, UpdateRequestType, ResponseType> {
	createScaffold(parentId: string | null): Promise<DataSourceResponse<CreateRequestType>>;
	get(unique: string): Promise<DataSourceResponse<ResponseType>>;
	insert(data: CreateRequestType): Promise<DataSourceResponse<CreateResponseType>>;
	update(unique: string, data: UpdateRequestType): Promise<DataSourceResponse<ResponseType>>;
	delete(unique: string): Promise<DataSourceResponse>;
}
