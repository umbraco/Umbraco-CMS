import { BehaviorSubject, Observable } from 'rxjs';
import { PropertyEditor } from '../../mocks/data/property-editor.data';

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
}
