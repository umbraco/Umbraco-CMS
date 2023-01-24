import type { Observable } from 'rxjs';

export interface UmbDataStoreIdentifiers {
	key?: string;
	[more: string]: any;
}

export interface UmbDataStore {
	readonly storeAlias: string;
}

export interface UmbTreeStore<T> extends UmbDataStore {
	getTreeRoot(): Observable<Array<T>>;
	getTreeItemChildren(key: string): Observable<Array<T>>;
}

export interface UmbContentStore<T> extends UmbDataStore {
	/**
	 * @description - Request data by key. The data is added to the store and is returned as an Observable.
	 * @param {string} key
	 * @return {*}  {(Observable<T>)}
	 * @memberof UmbDataStoreBase
	 */
	getByKey(key: string): Observable<T | undefined>;

	/**
	 * @description - Save data.
	 * @param {object} data
	 * @return {*}  {(Promise<void>)}
	 * @memberof UmbContentStore
	 */
	save(data: T[]): Promise<void>;
}
