import { BehaviorSubject, map, Observable } from 'rxjs';
import { PropertyEditorConfig } from '../../../mocks/data/property-editor-config.data';

export class UmbPropertyEditorConfigStore {
	private _items: BehaviorSubject<Array<PropertyEditorConfig>> = new BehaviorSubject(<Array<PropertyEditorConfig>>[]);
	public readonly items: Observable<Array<PropertyEditorConfig>> = this._items.asObservable();

	getByAlias(alias: string): Observable<PropertyEditorConfig | undefined> {
		// TODO: use Fetcher API.
		// TODO: only fetch if the data type is not in the store?
		fetch(`/umbraco/backoffice/property-editors/config/${alias}`)
			.then((res) => res.json())
			.then((data) => {
				this.update(data);
			});

		return this.items.pipe(map((items) => items.find((item) => item.propertyEditorAlias === alias)));
	}

	public update(updatedItems: Array<PropertyEditorConfig>) {
		const storedItems = this._items.getValue();
		const updated: PropertyEditorConfig[] = [...storedItems];

		updatedItems.forEach((updatedItem) => {
			const index = storedItems
				.map((storedItem) => storedItem.propertyEditorAlias)
				.indexOf(updatedItem.propertyEditorAlias);

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
