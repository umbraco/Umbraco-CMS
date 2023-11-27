import { UmbStore } from '@umbraco-cms/backoffice/store';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbTreeStore<T = any> extends UmbStore<T>, UmbApi {
	rootItems: Observable<Array<T>>;
	childrenOf: (parentUnique: string | null) => Observable<Array<T>>;
	// TODO: remove this one when all repositories are using an item store
	items: (uniques: Array<string>) => Observable<Array<T>>;
}
