import { BehaviorSubject, map, Observable } from 'rxjs';
import { DataTypeEntity } from '../../mocks/data/data-type.data';

export class UmbDataTypeStore {
	private _dataTypes: BehaviorSubject<Array<DataTypeEntity>> = new BehaviorSubject(<Array<DataTypeEntity>>[]);
	public readonly dataTypes: Observable<Array<DataTypeEntity>> = this._dataTypes.asObservable();

	getByKey(key: string): Observable<DataTypeEntity | null> {
		// TODO: use Fetcher API.
		// TODO: only fetch if the data type is not in the store?
		fetch(`/umbraco/backoffice/data-type/by-key/${key}`)
			.then((res) => res.json())
			.then((data) => {
				this._updateStore(data);
			});

		return this.dataTypes.pipe(
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
			this._updateStore(json);
		} catch (error) {
			console.error('Save Data Type error', error);
		}
	}

	private _updateStore(fetchedDataTypes: Array<DataTypeEntity>) {
		const storedDataTypes = this._dataTypes.getValue();
		const updated: DataTypeEntity[] = [...storedDataTypes];

		fetchedDataTypes.forEach((fetchedDataType) => {
			const index = storedDataTypes.map((storedNode) => storedNode.id).indexOf(fetchedDataType.id);

			if (index !== -1) {
				// If the data type is already in the store, update it
				updated[index] = fetchedDataType;
			} else {
				// If the data type is not in the store, add it
				updated.push(fetchedDataType);
			}
		});

		this._dataTypes.next([...updated]);
	}
}
