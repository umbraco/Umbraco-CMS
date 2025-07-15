import { UmbStoreUpdateEvent } from './events/index.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { type Observable, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

/**
 * The base class for a store that holds an object.
 */
export class UmbStoreObjectBase<T> extends UmbContextBase implements UmbApi {
	protected _data;

	constructor(host: UmbControllerHost, storeAlias: string, initialData?: T) {
		super(host, storeAlias);
		this._data = new UmbObjectState<T | null>(initialData ?? null);
	}

	/**
	 * Updates the store with the given data
	 * @param data - The (partial) data to update the store with
	 * @memberof UmbStoreObjectBase
	 */
	update(data: Partial<T>) {
		this._data.update(data);
		this.dispatchEvent(new UmbStoreUpdateEvent([]));
	}

	/**
	 * Returns the current state of the store
	 * @memberof UmbStoreObjectBase
	 */
	getState() {
		return this._data.getValue();
	}

	/**
	 * Returns an observable of the store
	 * @memberof UmbStoreObjectBase
	 */
	all() {
		return this._data.asObservable();
	}

	/**
	 * Returns an observable of a part of the store
	 * @param key - The key of the part to return
	 * @memberof UmbStoreObjectBase
	 */
	part<Part extends keyof T>(key: Part): Observable<T[Part]> {
		return this._data.asObservablePart((data) => data![key]);
	}

	/**
	 * Destroys the store
	 * @memberof UmbStoreObjectBase
	 */
	override destroy() {
		this._data.destroy();
	}
}
