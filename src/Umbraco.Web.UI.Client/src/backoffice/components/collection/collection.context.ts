import { BehaviorSubject, Observable } from 'rxjs';

export interface UmbCollectionContext {
	readonly selectable: Observable<boolean>;
	readonly selection: Observable<Array<string>>;
	readonly entityKey: string;
	setSelectable(value: boolean): void;
	setSelection(value: Array<string>): void;
	select(key: string): void;
}

export class UmbCollectionContextBase implements UmbCollectionContext {
	private _selectable: BehaviorSubject<boolean> = new BehaviorSubject(false);
	public readonly selectable: Observable<boolean> = this._selectable.asObservable();

	private _selection: BehaviorSubject<Array<string>> = new BehaviorSubject(<Array<string>>[]);
	public readonly selection: Observable<Array<string>> = this._selection.asObservable();

	public entityKey = '';

	constructor(entityKey: string) {
		this.entityKey = entityKey;
	}

	public setSelectable(value: boolean) {
		this._selectable.next(value);
	}

	public setSelection(value: Array<string>) {
		if (!value) return;
		this._selection.next(value);
	}

	public select(key: string) {
		const selection = [...this._selection.getValue(), key];
		this._selection.next(selection);
	}

	public deselect(key: string) {
		const selection = this._selection.getValue();
		this._selection.next(selection.filter((x) => x !== key));
	}
}
