import { BehaviorSubject, Observable } from 'rxjs';

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

	public abstract readonly storeAlias:string;

	protected _items: BehaviorSubject<Array<T>> = new BehaviorSubject(<Array<T>>[]);
	public readonly items: Observable<Array<T>> = this._items.asObservable();

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
	 * @description - Update the store with new items. Existing items are updated, new items are added. Existing items are matched by the compareKey.
	 * @param {Array<T>} items
	 * @param {keyof T} [compareKey='key']
	 * @memberof UmbDataStoreBase
	 */
	public updateItems(items: Array<T>, compareKey: keyof T = 'key'): void {
		const storedItems = [...this._items.getValue()];

		items.forEach((item) => {
			const index = storedItems.map((storedItem) => storedItem[compareKey]).indexOf(item[compareKey]);

			// If the node is in the store, update it
			if (index !== -1) {
				const storedItem = storedItems[index];

				for (const [key] of Object.entries(item)) {
					// eslint-disable-next-line @typescript-eslint/ban-ts-comment
					//@ts-ignore
					storedItem[key] = item[key];
				}
			} else {
				// If the node is not in the store, add it
				storedItems.push(item);
			}
		});

		this._items.next([...storedItems]);
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
	 * @return {*}  {(Observable<T | null>)}
	 * @memberof UmbDataStoreBase
	 */
	abstract getByKey(key: string): Observable<T | null>;

	/**
	 * @description - Save data.
	 * @param {object} data
	 * @return {*}  {(Promise<void>)}
	 * @memberof UmbNodeStoreBase
	 */
	abstract save(data: T[]): Promise<void>;
}