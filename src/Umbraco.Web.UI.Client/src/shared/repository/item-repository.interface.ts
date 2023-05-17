import type { Observable } from 'rxjs';
import { ItemResponseModelBaseModel, ProblemDetailsModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbItemRepository<ItemType extends ItemResponseModelBaseModel> {
	requestItems: (uniques: string[]) => Promise<{
		data?: Array<ItemType> | undefined;
		error?: ProblemDetailsModel | undefined;
		asObservable?: () => Observable<Array<ItemType>>;
	}>;
	items: (uniques: string[]) => Promise<Observable<Array<ItemType>>>;
}
