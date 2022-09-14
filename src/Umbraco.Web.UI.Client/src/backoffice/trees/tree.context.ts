import { BehaviorSubject, Observable } from 'rxjs';
import type { ManifestTree } from '../../core/models';

export interface UmbTreeContext {
	tree: ManifestTree;
	readonly selectable: Observable<boolean>;
	readonly selection: Observable<Array<string>>;
	setSelectable(value: boolean): void;
	setSelection(value: Array<string>): void;
	select(key: string): void;
}

export class UmbTreeContextBase implements UmbTreeContext {
	public tree: ManifestTree;
	public rootKey = '';

	private _selectable: BehaviorSubject<boolean> = new BehaviorSubject(false);
	public readonly selectable: Observable<boolean> = this._selectable.asObservable();

	private _selection: BehaviorSubject<Array<string>> = new BehaviorSubject(<Array<string>>[]);
	public readonly selection: Observable<Array<string>> = this._selection.asObservable();

	constructor(tree: ManifestTree) {
		this.tree = tree;
	}

	public setSelectable(value: boolean) {
		this._selectable.next(value);
	}

	public setSelection(value: Array<string>) {
		if (!value) return;
		this._selection.next(value);
	}

	public select(key: string) {
		const selection = this._selection.getValue();
		this._selection.next([...selection, key]);
	}
}
