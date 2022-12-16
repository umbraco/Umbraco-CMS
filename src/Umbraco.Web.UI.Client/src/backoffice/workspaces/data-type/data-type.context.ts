import { BehaviorSubject, Observable } from 'rxjs';
import type { DataTypeDetails } from '@umbraco-cms/models';

export class UmbDataTypeContext {
	// TODO: figure out how fine grained we want to make our observables.
	private _data = new BehaviorSubject<DataTypeDetails>({
		key: '',
		name: '',
		icon: '',
		type: 'dataType',
		hasChildren: false,
		parentKey: '',
		propertyEditorModelAlias: '',
		propertyEditorUIAlias: '',
		data: [],
	});
	public readonly data: Observable<DataTypeDetails> = this._data.asObservable();

	constructor(dataType: DataTypeDetails) {
		if (!dataType) return;
		this._data.next(dataType);
	}

	// TODO: figure out how we want to update data
	public update(data: Partial<DataTypeDetails>) {
		this._data.next({ ...this._data.getValue(), ...data });
	}

	public getData() {
		return this._data.getValue();
	}

	public setPropertyValue(propertyAlias: string, value: any) {
		const data = this._data.getValue();
		const property = data.data.find((p) => p.alias === propertyAlias);
		if (!property) return;

		property.value = value;
		this._data.next({ ...data });
	}
}
