import type { UmbStore } from '../store.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbItemStore<T extends { unique: string }> extends UmbStore<T>, UmbApi {
	items: (uniques: Array<string>) => Observable<Array<T>>;
}
