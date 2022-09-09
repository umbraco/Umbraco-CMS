import { BehaviorSubject, Observable } from 'rxjs';
import { DataTypeEntity } from '../../../mocks/data/data-type.data';

export class UmbDataTypeContext {
	// TODO: figure out how fine grained we want to make our observables.
	private _data = new BehaviorSubject<DataTypeEntity>({
		key: '',
		name: '',
		icon: '',
		type: '',
		hasChildren: false,
		parentKey: '',
		isTrashed: false,
		propertyEditorUIAlias: '',
	});
	public readonly data: Observable<DataTypeEntity> = this._data.asObservable();

	constructor(dataType: DataTypeEntity) {
		if (!dataType) return;
		this._data.next(dataType);
	}

	// TODO: figure out how we want to update data
	public update(data: Partial<DataTypeEntity>) {
		this._data.next({ ...this._data.getValue(), ...data });
	}

	public getData() {
		return this._data.getValue();
	}
}
