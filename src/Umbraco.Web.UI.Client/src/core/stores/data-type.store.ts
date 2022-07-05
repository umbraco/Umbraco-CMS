import { BehaviorSubject, map, Observable } from 'rxjs';
import { DataTypeEntity } from '../../mocks/data/content.data';
import { data } from '../../mocks/data/data-type.data';

export class UmbDataTypeStore {
	private _dataTypes: BehaviorSubject<Array<DataTypeEntity>> = new BehaviorSubject(<Array<DataTypeEntity>>[]);
	public readonly dataTypes: Observable<Array<DataTypeEntity>> = this._dataTypes.asObservable();

	constructor() {
		this._dataTypes.next(data);
	}

	getById(id: number): Observable<DataTypeEntity | null> {
		// no fetch..

		// TODO: make pipes prettier/simpler/reuseable
		return this.dataTypes.pipe(
			map((dataTypes: Array<DataTypeEntity>) => dataTypes.find((node: DataTypeEntity) => node.id === id) || null)
		);
	}

	getByKey(key: string): Observable<DataTypeEntity | null> {
		// no fetch..

		// TODO: make pipes prettier/simpler/reuseable
		return this.dataTypes.pipe(
			map((dataTypes: Array<DataTypeEntity>) => dataTypes.find((node: DataTypeEntity) => node.key === key) || null)
		);
	}
}
