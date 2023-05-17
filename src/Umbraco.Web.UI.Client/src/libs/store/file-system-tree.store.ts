import { FileSystemTreeItemPresentationModel } from 'src/libs/backend-api';
import { UmbArrayState } from 'src/libs/observable-api';
import { UmbStoreBase, UmbTreeStore } from 'src/libs/store';
import { UmbControllerHostElement } from 'src/libs/controller-api';

/**
 * @export
 * @class UmbFileSystemTreeStore
 * @extends {UmbStoreBase}
 * @description - File System Tree Store
 */
export class UmbFileSystemTreeStore
	extends UmbStoreBase<FileSystemTreeItemPresentationModel>
	implements UmbTreeStore<FileSystemTreeItemPresentationModel>
{
	constructor(host: UmbControllerHostElement, storeAlias: string) {
		super(host, storeAlias, new UmbArrayState<FileSystemTreeItemPresentationModel>([], (x) => x.path));
	}

	/**
	 * An observable to observe the root items
	 * @memberof UmbFileSystemTreeStore
	 */
	rootItems = this._data.getObservablePart((items) => items.filter((item) => item.path?.includes('/') === false));

	/**
	 * Returns an observable to observe the children of a given parent
	 * @param {(string | null)} parentPath
	 * @return {*}
	 * @memberof UmbFileSystemTreeStore
	 */
	childrenOf(parentPath: string | null) {
		return this._data.getObservablePart((items) =>
			items.filter((item) => item.path?.startsWith(parentPath + '/') || parentPath === null)
		);
	}

	/**
	 * Returns an observable to observe the items with the given ids
	 * @param {Array<string>} paths
	 * @return {*}
	 * @memberof UmbFileSystemTreeStore
	 */
	items(paths: Array<string>) {
		return this._data.getObservablePart((items) => items.filter((item) => paths.includes(item.path ?? '')));
	}
}
