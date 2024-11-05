import type { UmbStore } from '../store.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbDetailStore<T extends UmbEntityModel> extends UmbStore<T>, UmbApi {
	byUnique: (unique: string) => Observable<T | undefined>;
}
