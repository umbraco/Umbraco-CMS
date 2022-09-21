import { map, Observable } from 'rxjs';
import { DataTypeDetails } from '../../../mocks/data/data-type.data';
import { UmbEntityStore } from '../entity.store';
import { UmbDataStoreBase } from '../store';

export class UmbDataTypeStore extends UmbDataStoreBase<DataTypeDetails> {
	private _entityStore: UmbEntityStore;

	constructor(entityStore: UmbEntityStore) {
		super();
		this._entityStore = entityStore;
	}

	getByKey(key: string): Observable<DataTypeDetails | null> {
		// TODO: use Fetcher API.
		// TODO: only fetch if the data type is not in the store?
		fetch(`/umbraco/backoffice/data-type/details/${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.update(data);
			});

		return this.items.pipe(
			map((dataTypes: Array<DataTypeDetails>) => dataTypes.find((node: DataTypeDetails) => node.key === key) || null)
		);
	}

	async save(dataTypes: Array<DataTypeDetails>) {
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

	async trash(keys: string[]) {
		const res = await fetch('/umbraco/backoffice/data-type/trash', {
			method: 'POST',
			body: JSON.stringify(keys),
			headers: {
				'Content-Type': 'application/json',
			},
		});
		const data = await res.json();
		this.update(data);
		this._entityStore.update(data);
	}
}
