import { UmbStore } from './store.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbDetailStore<T extends { unique: string }> extends UmbStore<T>, UmbApi {
	byUnique: (unique: string) => Observable<T | undefined>;
}
