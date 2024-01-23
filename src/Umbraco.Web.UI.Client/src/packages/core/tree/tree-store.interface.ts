import { UmbTreeItemModelBase } from './types.js';
import { UmbStore } from '@umbraco-cms/backoffice/store';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbTreeStore<TreeItemType extends UmbTreeItemModelBase> extends UmbStore<TreeItemType>, UmbApi {
	rootItems: Observable<Array<TreeItemType>>;
	childrenOf: (parentUnique: string | null) => Observable<Array<TreeItemType>>;
	// TODO: remove this one when all repositories are using an item store
	items: (uniques: Array<string>) => Observable<Array<TreeItemType>>;
}
