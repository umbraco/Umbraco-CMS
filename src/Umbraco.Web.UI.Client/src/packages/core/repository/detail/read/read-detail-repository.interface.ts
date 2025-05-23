import type { UmbRepositoryResponseWithAsObservable } from '../../types.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbReadDetailRepository<DetailModelType> extends UmbApi {
	requestByUnique(unique: string): Promise<UmbRepositoryResponseWithAsObservable<DetailModelType>>;
	byUnique(unique: string): Promise<Observable<DetailModelType | undefined>>;
}
