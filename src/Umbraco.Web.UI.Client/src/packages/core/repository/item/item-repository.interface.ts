import type { UmbRepositoryResponseWithAsObservable } from '../types.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbItemRepository<ItemType> extends UmbApi {
	requestItems: (uniques: string[]) => Promise<UmbRepositoryResponseWithAsObservable<ItemType[] | undefined>>;
	items: (uniques: string[]) => Promise<Observable<Array<ItemType>> | undefined>;
}
