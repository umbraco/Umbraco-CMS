import type { Observable } from 'rxjs';
import type { ManifestTree } from '@umbraco-cms/models';
import { UniqueBehaviorSubject } from 'src/core/observable-api/unique-behavior-subject';

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

	#selectable = new UniqueBehaviorSubject(false);
	public readonly selectable = this.#selectable.asObservable();

	#selection = new UniqueBehaviorSubject(<Array<string>>[]);
	public readonly selection = this.#selection.asObservable();

	constructor(tree: ManifestTree) {
		this.tree = tree;
	}

	public setSelectable(value: boolean) {
		this.#selectable.next(value);
	}

	public setSelection(value: Array<string>) {
		if (!value) return;
		this.#selection.next(value);
	}

	public select(key: string) {
		const oldSelection = this.#selection.getValue();
		if(oldSelection.indexOf(key) !== -1) return;

		const selection = [...oldSelection, key];
		this.#selection.next(selection);
	}

	public deselect(key: string) {
		const selection = this.#selection.getValue();
		this.#selection.next(selection.filter((x) => x !== key));
	}
}
