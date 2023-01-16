import type { Observable } from 'rxjs';
import { UmbControllerHostInterface } from '../controller/controller-host.mixin';
import { UniqueBehaviorSubject } from '../observable-api/unique-behavior-subject';

export interface UmbDataStoreIdentifiers {
	key?: string;
	[more: string]: any;
}

export interface UmbDataStore<T> {
	readonly storeAlias: string;
	readonly items: Observable<Array<T>>;
	updateItems(items: Array<T>): void;
}

export interface UmbTreeDataStore<T> extends UmbDataStore<T> {
	getTreeRoot(): Observable<Array<T>>;
	getTreeItemChildren(key: string): Observable<Array<T>>;
}

/**
 * @export
 * @class UmbDataStoreBase
 * @implements {UmbDataStore<T>}
 * @template T
 * @description - Base class for Data Stores
 */
export abstract class UmbDataStoreBase<T extends UmbDataStoreIdentifiers> implements UmbDataStore<T> {
	public abstract readonly storeAlias: string;

	protected _items = new UniqueBehaviorSubject(<Array<T>>[]);
	public readonly items = this._items.asObservable();

	protected host: UmbControllerHostInterface;

	constructor(host: UmbControllerHostInterface) {
		this.host = host;
	}

	/**
	 * @description - Delete items from the store.
	 * @param {Array<string>} keys
	 * @memberof UmbDataStoreBase
	 */
	public deleteItems(keys: Array<string>): void {
		const remainingItems = this._items.getValue().filter((item) => item.key && keys.includes(item.key) === false);
		this._items.next(remainingItems);
	}

	/**
	 * @description - Update the store with new items. Existing items are updated, new items are added, old are kept. Items are matched by the compareKey.
	 * @param {Array<T>} items
	 * @param {keyof T} [compareKey='key']
	 * @memberof UmbDataStoreBase
	 */
	public updateItems(items: Array<T>, compareKey: keyof T = 'key'): void {
		const newData = [...this._items.getValue()];
		items.forEach((newItem) => {
			const storedItemIndex = newData.findIndex((item) => item[compareKey] === newItem[compareKey]);
			if (storedItemIndex !== -1) {
				newData[storedItemIndex] = newItem;
			} else {
				newData.push(newItem);
			}
		});

		this._items.next(newData);
	}
}

/**
 * @export
 * @class UmbNodeStoreBase
 * @implements {UmbDataStore<T>}
 * @template T
 * @description - Base class for Data Stores
 */
export abstract class UmbNodeStoreBase<T extends UmbDataStoreIdentifiers> extends UmbDataStoreBase<T> {
	/**
	 * @description - Request data by key. The data is added to the store and is returned as an Observable.
	 * @param {string} key
	 * @return {*}  {(Observable<unknown>)}
	 * @memberof UmbDataStoreBase
	 */
	abstract getByKey(key: string): Observable<unknown>;

	/**
	 * @description - Save data.
	 * @param {object} data
	 * @return {*}  {(Promise<void>)}
	 * @memberof UmbNodeStoreBase
	 */
	abstract save(data: T[]): Promise<void>;
}
