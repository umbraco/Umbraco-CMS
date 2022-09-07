import { BehaviorSubject, map, Observable } from 'rxjs';
import type { ManifestTree } from '../../core/models';
import type { UmbEntityStore } from '../../core/stores/entity.store';
import { Entity } from '../../mocks/data/entity.data';

export interface UmbTreeContext {
	tree: ManifestTree;
	rootKey: string;
	entityStore: UmbEntityStore;
	readonly selectable: Observable<boolean>;
	readonly selection: Observable<Array<string>>;
	rootChanges?(key: string): Observable<Entity[]>;
	childrenChanges?(key: string): Observable<Entity[]>;
	setSelectable(value: boolean): void;
	select(key: string): void;
}

export class UmbTreeContextBase implements UmbTreeContext {
	public tree: ManifestTree;
	public entityStore: UmbEntityStore;
	public rootKey = '';

	private _selectable: BehaviorSubject<boolean> = new BehaviorSubject(false);
	public readonly selectable: Observable<boolean> = this._selectable.asObservable();

	private _selection: BehaviorSubject<Array<string>> = new BehaviorSubject(<Array<string>>[]);
	public readonly selection: Observable<Array<string>> = this._selection.asObservable();

	constructor(tree: ManifestTree, entityStore: UmbEntityStore) {
		this.tree = tree;
		this.entityStore = entityStore;
	}

	public rootChanges() {
		return this.entityStore.items.pipe(
			map((items) => items.filter((item) => item.key === this.rootKey && !item.isTrashed))
		);
	}

	public childrenChanges(key: string) {
		return this.entityStore.items.pipe(
			map((items) => items.filter((item) => item.parentKey === key && !item.isTrashed))
		);
	}

	public setSelectable(value: boolean) {
		this._selectable.next(value);
	}

	public setSelection(value: Array<string>) {
		this._selection.next(value);
	}

	public select(key: string) {
		const selection = this._selection.getValue();
		this._selection.next([...selection, key]);
	}
}
