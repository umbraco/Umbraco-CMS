import { BehaviorSubject, Observable } from 'rxjs';
import { Entity } from '../../mocks/data/entity.data';

export interface UmbDataStore<T> {
	readonly items: Observable<Array<T>>;
	update(items: Array<T>): void;
}

export class UmbDataStoreBase<T extends Entity> implements UmbDataStore<T> {
	private _items: BehaviorSubject<Array<T>> = new BehaviorSubject(<Array<T>>[]);
	public readonly items: Observable<Array<T>> = this._items.asObservable();

	public update(updatedItems: Array<T>) {
		const storedItems = this._items.getValue();
		const updated: T[] = [...storedItems];

		updatedItems.forEach((updatedItem) => {
			const index = storedItems.map((storedItem) => storedItem.key).indexOf(updatedItem.key);

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
