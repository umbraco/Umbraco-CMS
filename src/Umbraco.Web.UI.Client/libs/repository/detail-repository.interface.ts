import type { ProblemDetailsModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbRepositoryErrorResponse {
	error?: ProblemDetailsModel;
}
export interface UmbRepositoryResponse<T> extends UmbRepositoryErrorResponse {
	data?: T;
}

export interface UmbDetailRepository<CreateRequestType = any, UpdateRequestType = any, ResponseType = any> {
	createScaffold(parentId: string | null): Promise<UmbRepositoryResponse<CreateRequestType>>;
	requestById(id: string): Promise<UmbRepositoryResponse<ResponseType>>;
	create(data: CreateRequestType): Promise<UmbRepositoryErrorResponse>;
	save(id: string, data: UpdateRequestType): Promise<UmbRepositoryErrorResponse>;
	delete(id: string): Promise<UmbRepositoryErrorResponse>;
}
