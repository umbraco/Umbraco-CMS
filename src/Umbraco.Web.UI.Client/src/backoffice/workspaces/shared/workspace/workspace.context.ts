import { BehaviorSubject } from "rxjs";

type WorkspaceContextData = {
	entityKey: string,
	entityType: string,
}

export class UmbWorkspaceContext {

	private _data = new BehaviorSubject<WorkspaceContextData>({
		entityKey: '',
		entityType: '',
	});
	public readonly data = this._data.asObservable();


	public get entityKey() {
		return this.getData().entityKey;
	}
	public set entityKey(value) {
		this.update({entityKey:value});
	}


	public get entityType() {
		return this.getData().entityType;
	}
	public set entityType(value) {
		this.update({entityType:value});
	}

	public getData() {
		return this._data.getValue();
	}

	public update(data: Partial<WorkspaceContextData>) {
		this._data.next({ ...this._data.getValue(), ...data });
	}

}