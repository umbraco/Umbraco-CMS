import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { ItemResponseModelBaseModel, ProblemDetails } from '@umbraco-cms/backoffice/backend-api';

export interface UmbItemRepository<ItemType extends ItemResponseModelBaseModel> {
	requestItems: (uniques: string[]) => Promise<{
		data?: Array<ItemType> | undefined;
		error?: ProblemDetails | undefined;
		asObservable?: () => Observable<Array<ItemType>>;
	}>;
	items: (uniques: string[]) => Promise<Observable<Array<ItemType>>>;
}
