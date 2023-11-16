import type { DataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbDataSource<
	CreateRequestType,
	CreateResponseType,
	UpdateRequestType,
	ResponseType,
	CreateScaffoldPresetType = Partial<CreateRequestType>,
> {
	createScaffold(
		parentId: string | null,
		preset?: Partial<CreateRequestType> | CreateScaffoldPresetType,
	): Promise<DataSourceResponse<CreateRequestType>>;
	create(data: CreateRequestType): Promise<DataSourceResponse<CreateResponseType>>;
	read(unique: string): Promise<DataSourceResponse<ResponseType>>;
	update(unique: string, data: UpdateRequestType): Promise<DataSourceResponse<ResponseType>>;
	delete(unique: string): Promise<DataSourceResponse>;
}
