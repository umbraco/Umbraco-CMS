import { BehaviorSubject, Observable } from "rxjs";


export abstract class UmbWorkspaceContext<DataType> {


	protected _target!:HTMLElement;


	// TODO: figure out how fine grained we want to make our observables.
	// TODO: add interface
	protected _data!:BehaviorSubject<DataType>;
	public readonly data: Observable<DataType>;


	constructor(target:HTMLElement, defaultData: DataType) {
		this._target = target;

		this._data = new BehaviorSubject<DataType>(defaultData);
		this.data = this._data.asObservable();
	}


	public getData() {
		return this._data.getValue();
	}

	public update(data: Partial<DataType>) {
		this._data.next({ ...this.getData(), ...data });
	}




	// TODO: how can we make sure to call this.
	public destroy(): void {
		this._data.unsubscribe();
	}

}

