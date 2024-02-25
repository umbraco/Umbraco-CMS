import type { UmbTreeItemModelBase } from '../types.js';
import type { UmbPagedModel } from '@umbraco-cms/backoffice/repository';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { ProblemDetails } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

/**
 * Interface for a tree repository.
 * @export
 * @interface UmbTreeRepository
 * @extends {UmbApi}
 * @template TreeItemType
 * @template TreeRootType
 */
export interface UmbTreeRepository<
	TreeItemType extends UmbTreeItemModelBase = UmbTreeItemModelBase,
	TreeRootType extends UmbTreeItemModelBase = UmbTreeItemModelBase,
> extends UmbApi {
	/**
	 * Requests the root of the tree.
	 * @memberof UmbTreeRepository
	 */
	requestTreeRoot: () => Promise<{
		data?: TreeRootType;
		error?: ProblemDetails;
	}>;

	/**
	 * Requests the root items of the tree.
	 * @memberof UmbTreeRepository
	 */
	requestRootTreeItems: () => Promise<{
		data?: UmbPagedModel<TreeItemType>;
		error?: ProblemDetails;
		asObservable?: () => Observable<TreeItemType[]>;
	}>;

	/**
	 * Requests the items of a item in the tree.
	 * @param {(string | null)} parentUnique
	 * @memberof UmbTreeRepository
	 */
	requestTreeItemsOf: (parentUnique: string | null) => Promise<{
		data?: UmbPagedModel<TreeItemType>;
		error?: ProblemDetails;
		asObservable?: () => Observable<TreeItemType[]>;
	}>;

	/**
	 * Returns an observable of the root items of the tree.
	 * @memberof UmbTreeRepository
	 */
	rootTreeItems: () => Promise<Observable<TreeItemType[]>>;

	/**
	 * Returns an observable of the children of the given parent item.
	 * @param {(string | null)} parentUnique
	 * @memberof UmbTreeRepository
	 */
	treeItemsOf: (parentUnique: string | null) => Promise<Observable<TreeItemType[]>>;
}
