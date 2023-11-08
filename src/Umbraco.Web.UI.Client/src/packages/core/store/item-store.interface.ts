import { UmbStore } from './store.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { ItemResponseModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import type { ExtensionApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbItemStore<T extends ItemResponseModelBaseModel = any> extends UmbStore<T>, ExtensionApi {
	items: (uniques: Array<string>) => Observable<Array<T>>;
}
