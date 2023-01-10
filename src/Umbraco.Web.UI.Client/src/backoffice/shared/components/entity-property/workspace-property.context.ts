import { BehaviorSubject, Observable } from "rxjs";

export type WorkspacePropertyData<ValueType> = {
    alias?: string | null;
    label?: string | null;
    value?: ValueType | null;
};



export class UmbWorkspacePropertyContext<ValueType> {


	private _data: BehaviorSubject<WorkspacePropertyData<ValueType>>;
	public readonly data: Observable<WorkspacePropertyData<ValueType>>;


	#defaultValue!: ValueType | null;


	constructor(defaultValue: ValueType | null) {

		this.#defaultValue = defaultValue;

		// TODO: How do we connect this value with parent context?
		// Ensuring the property editor value-property is updated...

		this._data = new BehaviorSubject({value: defaultValue} as WorkspacePropertyData<ValueType>);
		this.data = this._data.asObservable();
	}

    /*
	hostConnected() {

	}

	hostDisconnected() {
		
	}
    */

	public getData() {
		return this._data.getValue();
	}

	
	public update(data: Partial<WorkspacePropertyData<ValueType>>) {
		this._data.next({ ...this.getData(), ...data });
	}

	public resetValue() {
		console.log("property context reset")
		
		this.update({value: this.#defaultValue})
	}


	// TODO: how can we make sure to call this.
	public destroy(): void {
		this._data.unsubscribe();
	}

}

