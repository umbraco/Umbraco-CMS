import { UmbStore } from './store.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbItemStore<T = any> extends UmbStore<T>, UmbApi {
	items: (uniques: Array<string>) => Observable<Array<T>>;
}
