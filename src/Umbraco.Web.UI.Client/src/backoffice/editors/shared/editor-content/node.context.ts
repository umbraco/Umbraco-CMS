import { BehaviorSubject, Observable } from 'rxjs';

export class UmbNodeContext {
	// TODO: figure out how fine grained we want to make our observables.
	// TODO: add interface
	private _data = new BehaviorSubject<any>({
		key: '',
		name: '',
		icon: '',
		type: '',
		hasChildren: false,
		parentKey: '',
		isTrashed: false,
		properties: [
			{
				alias: '',
				label: '',
				description: '',
				dataTypeKey: '',
			},
		],
		data: [
			{
				alias: '',
				value: '',
			},
		],
		variants: [
			{
				name: '',
			},
		],
	});
	public readonly data: Observable<any> = this._data.asObservable();

	constructor(node: any) {
		if (!node) return;
		this._data.next(node);
	}

	// TODO: figure out how we want to update data
	public update(data: Partial<any>) {
		this._data.next({ ...this._data.getValue(), ...data });
	}

	public getData() {
		return this._data.getValue();
	}
}
