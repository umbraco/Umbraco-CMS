import { BehaviorSubject, Observable } from 'rxjs';

export interface UmbDataStoreIdentifiers {
	key?: string;
	[more: string]: any;
}

export interface UmbDataStore<T> {
	readonly items: Observable<Array<T>>;
	update(items: Array<T>): void;
}

/**
 * @export
 * @class UmbDataStoreBase
 * @implements {UmbDataStore<T>}
 * @template T
 * @description - Base class for Data Stores
 */
export class UmbDataStoreBase<T extends UmbDataStoreIdentifiers> implements UmbDataStore<T> {
	private _items: BehaviorSubject<Array<T>> = new BehaviorSubject(<Array<T>>[]);
	public readonly items: Observable<Array<T>> = this._items.asObservable();

	/**
	 * @description - Update the store with new items. Existing items are updated, new items are added. Existing items are matched by the compareKey.
	 * @param {Array<T>} updatedItems
	 * @param {keyof T} [compareKey='key']
	 * @memberof UmbDataStoreBase
	 */
	public update(updatedItems: Array<T>, compareKey: keyof T = 'key'): void {
		const storedItems = this._items.getValue();
		const updated: T[] = [...storedItems];

		updatedItems.forEach((updatedItem) => {
			const index = storedItems.map((storedItem) => storedItem[compareKey]).indexOf(updatedItem[compareKey]);

			if (index !== -1) {
				const itemKeys = Object.keys(storedItems[index]);

				const newItem = {};

				for (const [key] of Object.entries(updatedItem)) {
					if (itemKeys.indexOf(key) !== -1) {
						// eslint-disable-next-line @typescript-eslint/ban-ts-comment
						// @ts-ignore
						newItem[key] = updatedItem[key];
					}
				}

				// eslint-disable-next-line @typescript-eslint/ban-ts-comment
				// @ts-ignore
				updated[index] = newItem;
			} else {
				// If the node is not in the store, add it
				updated.push(updatedItem);
			}
		});

		this._items.next([...updated]);
	}
}
