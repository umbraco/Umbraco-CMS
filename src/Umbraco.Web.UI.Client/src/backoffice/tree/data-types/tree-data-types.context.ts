import { BehaviorSubject, map, Observable } from 'rxjs';
import { UmbEntityStore } from '../../../core/stores/entity.store';
import { UmbTreeContext } from '../tree.context';

export class UmbTreeDataTypesContext implements UmbTreeContext {
	public entityStore: UmbEntityStore;

	private _selectable: BehaviorSubject<boolean> = new BehaviorSubject(false);
	public readonly selectable: Observable<boolean> = this._selectable.asObservable();

	private _rootKey = '29d78e6c-c1bf-4c15-b820-d511c237ffae';

	constructor(entityStore: UmbEntityStore) {
		this.entityStore = entityStore;
	}

	public fetchRoot() {
		const data = {
			key: this._rootKey,
			name: 'Data Types',
			hasChildren: true,
			type: 'dataTypeRoot',
			icon: 'favorite',
			parentKey: '',
		};
		this.entityStore.update([data]);
		return this.entityStore.entities.pipe(map((items) => items.filter((item) => item.key === this._rootKey)));
	}

	public fetchChildren(key = '') {
		// TODO: figure out url structure
		fetch(`/umbraco/backoffice/entities/data-types?parentKey=${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.entityStore.update(data);
			});

		return this.entityStore.entities.pipe(map((items) => items.filter((item) => item.parentKey === key)));
	}

	public setSelectable(value: boolean) {
		this._selectable.next(value);
	}
}
