import type { Observable } from 'rxjs';
import type { ManifestTree, UmbTreeRepository } from '@umbraco-cms/models';
import { DeepState } from '@umbraco-cms/observable-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export interface UmbTreeContext {
	tree: ManifestTree;
	readonly selectable: Observable<boolean>;
	readonly selection: Observable<Array<string>>;
	setSelectable(value: boolean): void;
	setSelection(value: Array<string>): void;
	select(key: string): void;
}

export class UmbTreeContextBase implements UmbTreeContext {
	#host: UmbControllerHostInterface;
	public tree: ManifestTree;

	#selectable = new DeepState(false);
	public readonly selectable = this.#selectable.asObservable();

	#selection = new DeepState(<Array<string>>[]);
	public readonly selection = this.#selection.asObservable();

	repository?: UmbTreeRepository;

	constructor(host: UmbControllerHostInterface, tree: ManifestTree) {
		this.#host = host;
		this.tree = tree;

		if (this.tree.meta.repository) {
			this.repository = new this.tree.meta.repository(this.#host);
		}
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
		if (oldSelection.indexOf(key) !== -1) return;

		const selection = [...oldSelection, key];
		this.#selection.next(selection);
	}

	public deselect(key: string) {
		const selection = this.#selection.getValue();
		this.#selection.next(selection.filter((x) => x !== key));
	}

	public async getRoot() {
		if (!this.repository) {
			return { data: undefined, updates: undefined, error: undefined };
		}
		return this.repository?.getRoot();
	}

	public async getChildren(parentKey: string | null) {
		if (!this.repository) {
			return { data: undefined, updates: undefined, error: undefined };
		}

		return this.repository.getChildren(parentKey);
	}
}
