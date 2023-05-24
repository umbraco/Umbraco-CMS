import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { TreeItemPresentationModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbStore } from './store.interface.js';

export interface UmbTreeStore<T extends TreeItemPresentationModel = any> extends UmbStore<T> {
	rootItems: Observable<Array<T>>;
	childrenOf: (parentUnique: string | null) => Observable<Array<T>>;
	// TODO: remove this one when all repositories are using an item store
	items: (uniques: Array<string>) => Observable<Array<T>>;
}
