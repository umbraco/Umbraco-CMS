import type { UmbRepositoryErrorResponse, UmbRepositoryResponse } from '../types.js';
import type { UmbReadDetailRepository } from './read/index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbDetailRepositoryConstructor<DetailModelType> {
	new (host: UmbControllerHost): UmbDetailRepository<DetailModelType>;
}

export interface UmbDetailRepository<DetailModelType> extends UmbReadDetailRepository<DetailModelType>, UmbApi {
	createScaffold(preset?: Partial<DetailModelType>): Promise<UmbRepositoryResponse<DetailModelType>>;
	create(data: DetailModelType, parentUnique: string | null): Promise<UmbRepositoryResponse<DetailModelType>>;
	save(data: DetailModelType): Promise<UmbRepositoryResponse<DetailModelType>>;
	delete(unique: string): Promise<UmbRepositoryErrorResponse>;
}
