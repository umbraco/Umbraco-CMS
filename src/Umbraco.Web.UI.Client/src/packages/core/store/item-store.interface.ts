import { UmbStore } from './store.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { ItemResponseModelBaseModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbItemStore<T extends ItemResponseModelBaseModel = any> extends UmbStore<T> {
	items: (uniques: Array<string>) => Observable<Array<T>>;
}
