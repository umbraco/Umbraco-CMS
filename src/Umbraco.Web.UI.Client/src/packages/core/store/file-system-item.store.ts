import { UmbStoreBase } from './store-base.js';
import { UmbItemStore } from './item-store.interface.js';
import type { FileItemResponseModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * @export
 * @class UmbFileSystemItemStore
 * @extends {UmbStoreBase}
 * @description - Data Store for File system items
 */

export class UmbFileSystemItemStore<T extends FileItemResponseModelBaseModel>
	extends UmbStoreBase<T>
	implements UmbItemStore<T>
{
	constructor(host: UmbControllerHost, storeAlias: string) {
		super(host, storeAlias, new UmbArrayState<T>([], (x) => x.path));
	}

	/**
	 * Return an observable to observe file system items
	 * @param {Array<string>} paths
	 * @return {*}
	 * @memberof UmbFileSystemItemStore
	 */
	items(paths: Array<string>) {
		return this._data.asObservablePart((items) => items.filter((item) => paths.includes(item.path ?? '')));
	}
}
