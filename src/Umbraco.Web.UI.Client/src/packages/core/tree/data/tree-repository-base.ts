import type { UmbTreeItemModel, UmbTreeRootModel } from '../types.js';
import type { UmbTreeStore } from './tree-store.interface.js';
import type { UmbTreeRepository } from './tree-repository.interface.js';
import type { UmbTreeDataSource, UmbTreeDataSourceConstructor } from './tree-data-source.interface.js';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeRootItemsRequestArgs,
} from './types.js';
import { UmbRepositoryBase, type UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { of } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

/**
 * Base class for a tree repository.
 * @abstract
 * @class UmbTreeRepositoryBase
 * @augments {UmbRepositoryBase}
 * @implements {UmbTreeRepository}
 * @implements {UmbApi}
 * @template TreeItemType
 * @template TreeRootType
 */
export abstract class UmbTreeRepositoryBase<
		TreeItemType extends UmbTreeItemModel,
		TreeRootType extends UmbTreeRootModel,
		TreeRootItemsRequestArgsType extends UmbTreeRootItemsRequestArgs = UmbTreeRootItemsRequestArgs,
		TreeChildrenOfRequestArgsType extends UmbTreeChildrenOfRequestArgs = UmbTreeChildrenOfRequestArgs,
		TreeAncestorsOfRequestArgsType extends UmbTreeAncestorsOfRequestArgs = UmbTreeAncestorsOfRequestArgs,
	>
	extends UmbRepositoryBase
	implements
		UmbTreeRepository<
			TreeItemType,
			TreeRootType,
			TreeRootItemsRequestArgsType,
			TreeChildrenOfRequestArgsType,
			TreeAncestorsOfRequestArgsType
		>,
		UmbApi
{
	protected _init: Promise<unknown>;
	protected _treeStore?: UmbTreeStore<TreeItemType>;
	protected _treeSource: UmbTreeDataSource<TreeItemType>;

	/**
	 * Creates an instance of UmbTreeRepositoryBase.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @param {UmbTreeDataSourceConstructor<TreeItemType>} treeSourceConstructor - The constructor for the tree data source
	 * @param {(string | UmbContextToken<any, any> | undefined)} treeStoreContextAlias - The context alias for the tree store, if any
	 * @memberof UmbTreeRepositoryBase
	 */
	constructor(
		host: UmbControllerHost,
		treeSourceConstructor: UmbTreeDataSourceConstructor<TreeItemType>,
		treeStoreContextAlias?: string | UmbContextToken<any, any>,
	) {
		super(host);
		this._treeSource = new treeSourceConstructor(this);

		if (treeStoreContextAlias) {
			if (false === treeStoreContextAlias.toString().startsWith('Umb')) {
				new UmbDeprecation({
					deprecated: `TreeRepository "${this.constructor.name}" is using a tree store context with alias "${treeStoreContextAlias.toString()}".`,
					removeInVersion: '18.0.0',
					solution:
						'You do not need to supply a tree store context alias, as the tree repository will be queried each time it is needed.',
				}).warn();
			}

			// TODO: Remember to remove this in Umbraco 18, as the tree store will not be available in the repository anymore.
			this._init = this.consumeContext(treeStoreContextAlias, (instance) => {
				this._treeStore = instance;
			})
				.asPromise({ preventTimeout: true })
				// Ignore the error, we can assume that the flow was stopped (asPromise failed), but it does not mean that the consumption was not successful.
				.catch(() => undefined);
		} else {
			this._init = Promise.resolve();
		}
	}

	/**
	 * Request the tree root item
	 * @returns {*}
	 * @memberof UmbTreeRepositoryBase
	 */
	abstract requestTreeRoot(): Promise<UmbRepositoryResponse<TreeRootType>>;

	/**
	 * Requests root items of a tree
	 * @param args
	 * @returns {*}
	 * @memberof UmbTreeRepositoryBase
	 */
	async requestTreeRootItems(args: TreeRootItemsRequestArgsType) {
		await this._init;

		const { data, error } = await this._treeSource.getRootItems(args);

		if (!this._treeStore) {
			// If the tree store is not available, then we most likely are in a destructed setting.
			return {
				data,
				error,
				// Return an observable that does not emit any items, since the store is not available
				asObservable: () => undefined,
			};
		}

		if (data) {
			this._treeStore.appendItems(data.items);
		}

		return { data, error, asObservable: () => this._treeStore?.rootItems };
	}

	/**
	 * Requests tree items of a given parent
	 * @param {(string | null)} parentUnique
	 * @param args
	 * @returns {*}
	 * @memberof UmbTreeRepositoryBase
	 */
	async requestTreeItemsOf(args: TreeChildrenOfRequestArgsType) {
		if (!args.parent) throw new Error('Parent is missing');
		if (args.parent.unique === undefined) throw new Error('Parent unique is missing');
		if (args.parent.entityType === null) throw new Error('Parent entity type is missing');
		await this._init;

		const { data, error } = await this._treeSource.getChildrenOf(args);

		if (!this._treeStore) {
			// If the tree store is not available, then we most likely are in a destructed setting.
			return {
				data,
				error,
				// Return an observable that does not emit any items, since the store is not available
				asObservable: () => undefined,
			};
		}

		if (data) {
			this._treeStore.appendItems(data.items);
		}

		return { data, error, asObservable: () => this._treeStore?.childrenOf(args.parent.unique) };
	}

	/**
	 * Requests ancestors of a given item
	 * @param {UmbTreeAncestorsOfRequestArgs} args
	 * @returns {*}
	 * @memberof UmbTreeRepositoryBase
	 */
	async requestTreeItemAncestors(args: TreeAncestorsOfRequestArgsType) {
		if (args.treeItem.unique === undefined) throw new Error('Descendant unique is missing');
		await this._init;

		const { data, error } = await this._treeSource.getAncestorsOf(args);

		// TODO: implement observable for ancestor items in the store
		// TODO: Fix the type of error, it should be UmbApiError, but currently it is any.
		return { data, error: error as any };
	}

	/**
	 * Returns a promise with an observable of tree root items
	 * @returns {*}
	 * @memberof UmbTreeRepositoryBase
	 * @deprecated Use `requestTreeRootItems` instead. This method requires the tree store to be available, which is not always the case. It will be removed in Umbraco 18.
	 */
	async rootTreeItems() {
		await this._init;

		return this._treeStore?.rootItems ?? of([]);
	}

	/**
	 * Returns a promise with an observable of children items of a given parent
	 * @param {(string | null)} parentUnique
	 * @returns {*}
	 * @memberof UmbTreeRepositoryBase
	 * @deprecated Use `requestTreeItemsOf` instead. This method requires the tree store to be available, which is not always the case. It will be removed in Umbraco 18.
	 */
	async treeItemsOf(parentUnique: string | null) {
		if (parentUnique === undefined) throw new Error('Parent unique is missing');
		await this._init;

		return this._treeStore?.childrenOf(parentUnique) ?? of([]);
	}
}
