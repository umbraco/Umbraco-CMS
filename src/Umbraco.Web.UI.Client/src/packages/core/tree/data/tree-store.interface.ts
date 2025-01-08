import type { UmbTreeItemModelBase } from '../types.js';
import type { UmbStore } from '@umbraco-cms/backoffice/store';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

/**
 * Interface for a tree store.
 * @interface UmbTreeStore
 * @augments {UmbStore<TreeItemType>}
 * @augments {UmbApi}
 * @template TreeItemType
 */
export interface UmbTreeStore<TreeItemType extends UmbTreeItemModelBase> extends UmbStore<TreeItemType>, UmbApi {
	/**
	 * Returns an observable of the root items of the tree.
	 * @type {Observable<Array<TreeItemType>>}
	 * @memberof UmbTreeStore
	 */
	rootItems: Observable<Array<TreeItemType>>;

	/**
	 * Returns an observable of the children of the given parent item.
	 * @param {(string | null)} parentUnique
	 * @memberof UmbTreeStore
	 */
	childrenOf: (parentUnique: string | null) => Observable<Array<TreeItemType>>;
}
