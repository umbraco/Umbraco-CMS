import { map, Observable } from 'rxjs';
import { DataTypeEntity } from '../../mocks/data/data-type.data';
import { UmbEntityStore } from './entity.store';
import { UmbDataStoreBase } from './store';

export class UmbDataTypeStore extends UmbDataStoreBase<DataTypeEntity> {
	private _entityStore: UmbEntityStore;

	constructor(entityStore: UmbEntityStore) {
		super();
		this._entityStore = entityStore;
	}

	getByKey(key: string): Observable<DataTypeEntity | null> {
		// TODO: use Fetcher API.
		// TODO: only fetch if the data type is not in the store?
		fetch(`/umbraco/backoffice/data-type/by-key/${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.update(data);
			});

		return this.items.pipe(
			map((dataTypes: Array<DataTypeEntity>) => dataTypes.find((node: DataTypeEntity) => node.key === key) || null)
		);
	}

	async save(dataTypes: Array<DataTypeEntity>) {
		// TODO: use Fetcher API.
		try {
			const res = await fetch('/umbraco/backoffice/data-type/save', {
				method: 'POST',
				body: JSON.stringify(dataTypes),
				headers: {
					'Content-Type': 'application/json',
				},
			});
			const json = await res.json();
			this.update(json);
			this._entityStore.update(json);
		} catch (error) {
			console.error('Save Data Type error', error);
		}
	}
}
