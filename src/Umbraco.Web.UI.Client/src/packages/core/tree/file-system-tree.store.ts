import { type UmbTreeStore } from './tree-store.interface.js';
import { UmbFileSystemTreeItemModel } from './types.js';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

/**
 * @export
 * @class UmbFileSystemTreeStore
 * @extends {UmbStoreBase}
 * @description - File System Tree Store
 */
export class UmbFileSystemTreeStore
	extends UmbStoreBase<UmbFileSystemTreeItemModel>
	implements UmbTreeStore<UmbFileSystemTreeItemModel>
{
	constructor(host: UmbControllerHostElement, storeAlias: string) {
		super(host, storeAlias, new UmbArrayState<UmbFileSystemTreeItemModel>([], (x) => x.path));
	}

	/**
	 * An observable to observe the root items
	 * @memberof UmbFileSystemTreeStore
	 */
	rootItems = this._data.asObservablePart((items) => items.filter((item) => item.path?.includes('/') === false));

	/**
	 * Returns an observable to observe the children of a given parent
	 * @param {(string | null)} parentPath
	 * @return {*}
	 * @memberof UmbFileSystemTreeStore
	 */
	childrenOf(parentPath: string | null) {
		if (parentPath === null) {
			return this.rootItems;
		}

		return this._data.asObservablePart((items) =>
			items.filter((item) => {
				const pathCut = item.path?.substring(0, item.path?.lastIndexOf('/'));
				return parentPath === pathCut;
			}),
		);
	}

	/**
	 * Returns an observable to observe the items with the given ids
	 * @param {Array<string>} paths
	 * @return {*}
	 * @memberof UmbFileSystemTreeStore
	 */
	items(paths: Array<string>) {
		return this._data.asObservablePart((items) => items.filter((item) => paths.includes(item.path ?? '')));
	}
}
