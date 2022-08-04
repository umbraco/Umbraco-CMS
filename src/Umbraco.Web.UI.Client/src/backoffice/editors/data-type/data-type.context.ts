import { BehaviorSubject, Observable } from 'rxjs';
import { DataTypeEntity } from '../../../mocks/data/data-type.data';

export class UmbDataTypeContext {
	// TODO: figure out how fine grained we want to make our observables.
	private _data: BehaviorSubject<DataTypeEntity> = new BehaviorSubject({
		id: -1,
		key: '',
		name: '',
		propertyEditorUIAlias: '',
	});
	public readonly data: Observable<DataTypeEntity> = this._data.asObservable();

	constructor(dataType: DataTypeEntity) {
		this._data.next(dataType);
	}

	// TODO: figure out how we want to update data
	public update(data: any) {
		this._data.next({ ...this._data.getValue(), ...data });
	}

	public getData() {
		return this._data.getValue();
	}
}
