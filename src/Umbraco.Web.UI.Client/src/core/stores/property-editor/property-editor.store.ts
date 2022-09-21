import { BehaviorSubject, map, Observable } from 'rxjs';
import { PropertyEditor } from '../../../mocks/data/property-editor.data';

export class UmbPropertyEditorStore {
	private _items: BehaviorSubject<Array<PropertyEditor>> = new BehaviorSubject(<Array<PropertyEditor>>[]);
	public readonly items: Observable<Array<PropertyEditor>> = this._items.asObservable();

	getAll(): Observable<Array<PropertyEditor> | []> {
		// TODO: use Fetcher API.
		// TODO: only fetch if the data type is not in the store?
		fetch(`/umbraco/backoffice/property-editors/list`)
			.then((res) => res.json())
			.then((data) => {
				this._items.next(data);
			});

		return this.items;
	}

	getByAlias(alias: string): Observable<PropertyEditor | undefined> {
		// TODO: use Fetcher API.
		// TODO: only fetch if the data type is not in the store?
		fetch(`/umbraco/backoffice/property-editors/property-editor/${alias}`)
			.then((res) => res.json())
			.then((data) => {
				this.update(data);
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
