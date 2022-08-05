import { BehaviorSubject, Observable } from 'rxjs';
import { NodeEntity } from '../../../mocks/data/content.data';

export class UmbNodeContext {
	// TODO: figure out how fine grained we want to make our observables.
	private _data: BehaviorSubject<NodeEntity> = new BehaviorSubject({
		id: -1,
		key: '',
		name: '',
		alias: '',
		icon: '',
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
	});
	public readonly data: Observable<NodeEntity> = this._data.asObservable();

	constructor(node?: NodeEntity) {
		if (!node) return;
		this._data.next(node);
	}

	// TODO: figure out how we want to update data
	public update(data: Partial<NodeEntity>) {
		this._data.next({ ...this._data.getValue(), ...data });
	}

	public getData() {
		return this._data.getValue();
	}
}
