import { BehaviorSubject, Observable } from 'rxjs';
import { NodeEntity } from '../../../../mocks/data/node.data';

export class UmbNodeContext {
	// TODO: figure out how fine grained we want to make our observables.
	private _data = new BehaviorSubject<NodeEntity>({
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
	public readonly data: Observable<NodeEntity> = this._data.asObservable();

	constructor(node: NodeEntity) {
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
