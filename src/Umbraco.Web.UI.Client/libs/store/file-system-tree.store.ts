import { FileSystemTreeItemPresentationModel } from '@umbraco-cms/backoffice/backend-api';
import { ArrayState, partialUpdateFrozenArray } from '@umbraco-cms/backoffice/observable-api';
import { UmbStoreBase, UmbTreeStore } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbFileSystemTreeStore
 * @extends {UmbStoreBase}
 * @description - General Tree Data Store
 */
export class UmbFileSystemTreeStore extends UmbStoreBase implements UmbTreeStore<FileSystemTreeItemPresentationModel> {
	#data = new ArrayState<FileSystemTreeItemPresentationModel>([], (x) => x.path);

	/**
	 * Appends items to the store
	 * @param {Array<FileSystemTreeItemPresentationModel>} items
	 * @memberof UmbFileSystemTreeStore
	 */
	appendItems(items: Array<FileSystemTreeItemPresentationModel>) {
		this.#data.append(items);
	}

	/**
	 * Updates an item in the store
	 * @param {string} path
	 * @param {Partial<FileSystemTreeItemPresentationModel>} data
	 * @memberof UmbFileSystemTreeStore
	 */
	updateItem(path: string, data: Partial<FileSystemTreeItemPresentationModel>) {
		this.#data.appendOne(data)
	}

	/**
	 * Removes an item from the store
	 * @param {string} path
	 * @memberof UmbFileSystemTreeStore
	 */
	removeItem(path: string) {
		this.#data.removeOne(path);
	}

	/**
	 * An observable to observe the root items
	 * @memberof UmbFileSystemTreeStore
	 */
	rootItems = this.#data.getObservablePart((items) => items.filter((item) => item.path?.includes('/') === false));

	/**
	 * Returns an observable to observe the children of a given parent
	 * @param {(string | null)} parentPath
	 * @return {*}
	 * @memberof UmbFileSystemTreeStore
	 */
	childrenOf(parentPath: string | null) {
		return this.#data.getObservablePart((items) => items.filter((item) => item.path?.startsWith(parentPath + '/')));
	}

	/**
	 * Returns an observable to observe the items with the given keys
	 * @param {Array<string>} paths
	 * @return {*}
	 * @memberof UmbFileSystemTreeStore
	 */
	items(paths: Array<string>) {
		return this.#data.getObservablePart((items) => items.filter((item) => paths.includes(item.path ?? '')));
	}
}
