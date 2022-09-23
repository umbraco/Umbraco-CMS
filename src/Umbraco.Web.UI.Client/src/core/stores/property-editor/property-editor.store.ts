import { BehaviorSubject, map, Observable } from 'rxjs';
import type { PropertyEditor } from '../../models';
import { getPropertyEditorsList, getPropertyEditor } from '../../api/fetcher';

export class UmbPropertyEditorStore {
	private _items: BehaviorSubject<Array<PropertyEditor>> = new BehaviorSubject(<Array<PropertyEditor>>[]);
	public readonly items: Observable<Array<PropertyEditor>> = this._items.asObservable();

	getAll(): Observable<Array<PropertyEditor> | []> {
		// TODO: only fetch if the data type is not in the store?
		getPropertyEditorsList({})
			.then((res) => {
				this._items.next(res.data.propertyEditors);
			})
			.catch((err) => {
				console.log(err);
			});

		return this.items;
	}

	getByAlias(alias: string): Observable<PropertyEditor | undefined> {
		// TODO: only fetch if the data type is not in the store?
		getPropertyEditor({ propertyEditorAlias: alias })
			.then((res) => {
				this.update([res.data]);
			})
			.catch((err) => {
				console.log(err);
			});

		return this.items.pipe(map((items) => items.find((item) => item.alias === alias)));
	}

	public update(updatedItems: Array<PropertyEditor>) {
		const storedItems = this._items.getValue();
		const updated: PropertyEditor[] = [...storedItems];

		updatedItems.forEach((updatedItem) => {
			const index = storedItems.map((storedItem) => storedItem.alias).indexOf(updatedItem.alias);

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
