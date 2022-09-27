import { BehaviorSubject, map, Observable } from 'rxjs';
import { getPropertyEditorConfig } from '../../api/fetcher';
import type { PropertyEditorConfig } from '../../models';

export interface PropertyEditorConfigRef {
	propertyEditorAlias: string;
	config: PropertyEditorConfig;
}

export class UmbPropertyEditorConfigStore {
	private _items: BehaviorSubject<Array<PropertyEditorConfigRef>> = new BehaviorSubject(
		<Array<PropertyEditorConfigRef>>[]
	);
	public readonly items: Observable<Array<PropertyEditorConfigRef>> = this._items.asObservable();

	getByAlias(alias: string): Observable<PropertyEditorConfigRef | undefined> {
		// TODO: only fetch if the data type is not in the store?
		getPropertyEditorConfig({ propertyEditorAlias: alias })
			.then((res) => {
				const propertyEditorConfigRef: PropertyEditorConfigRef = { propertyEditorAlias: alias, config: res.data };
				this.update([propertyEditorConfigRef]);
			})
			.catch((err) => {
				console.log(err);
			});

		return this.items.pipe(map((items) => items.find((item) => item.propertyEditorAlias === alias)));
	}

	public update(updatedItems: Array<PropertyEditorConfigRef>) {
		const storedItems = this._items.getValue();
		const updated: PropertyEditorConfigRef[] = [...storedItems];

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
