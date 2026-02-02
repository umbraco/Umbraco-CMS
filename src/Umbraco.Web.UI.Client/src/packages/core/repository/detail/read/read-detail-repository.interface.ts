import type { UmbRepositoryResponse, UmbRepositoryResponseWithAsObservable } from '../../types.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbReadDetailRepository<DetailModelType> extends UmbApi {
	requestByUnique(unique: string): Promise<UmbRepositoryResponseWithAsObservable<DetailModelType | undefined>>;
	requestByUniques?(uniques: Array<string>): Promise<UmbRepositoryResponse<Array<DetailModelType> | undefined>>;
	byUnique?(unique: string): Promise<Observable<DetailModelType | undefined>>;
}
