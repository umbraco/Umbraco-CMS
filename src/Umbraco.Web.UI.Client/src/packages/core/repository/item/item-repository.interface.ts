import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbProblemDetails } from '@umbraco-cms/backoffice/resources';

export interface UmbItemRepository<ItemType> extends UmbApi {
	requestItems: (uniques: string[]) => Promise<{
		data?: Array<ItemType> | undefined;
		error?: UmbProblemDetails | undefined;
		asObservable?: () => Observable<Array<ItemType>>;
	}>;
	items: (uniques: string[]) => Promise<Observable<Array<ItemType>>>;
}
