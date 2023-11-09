import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { ItemResponseModelBaseModel, ProblemDetails } from '@umbraco-cms/backoffice/backend-api';
import { ExtensionApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbItemRepository<ItemType extends ItemResponseModelBaseModel> extends ExtensionApi {
	requestItems: (uniques: string[]) => Promise<{
		data?: Array<ItemType> | undefined;
		error?: ProblemDetails | undefined;
		asObservable?: () => Observable<Array<ItemType>>;
	}>;
	items: (uniques: string[]) => Promise<Observable<Array<ItemType>>>;
}
