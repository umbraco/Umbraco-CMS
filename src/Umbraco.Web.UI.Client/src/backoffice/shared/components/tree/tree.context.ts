import type { Observable } from 'rxjs';
import { UmbTreeRepository } from '@umbraco-cms/backoffice/repository';
import type { ManifestTree } from '@umbraco-cms/backoffice/extensions-registry';
import { UmbBooleanState, UmbArrayState, UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { createExtensionClass, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extensions-api';

export interface UmbTreeContext {
	tree: ManifestTree;
	readonly selectable: Observable<boolean>;
	readonly selection: Observable<Array<string>>;
	setSelectable(value: boolean): void;
	setMultiple(value: boolean): void;
	setSelection(value: Array<string>): void;
	select(id: string): void;
}

export class UmbTreeContextBase implements UmbTreeContext {
	host: UmbControllerHostElement;
	public tree: ManifestTree;

	#selectable = new UmbBooleanState(false);
	public readonly selectable = this.#selectable.asObservable();

	#multiple = new UmbBooleanState(false);
	public readonly multiple = this.#multiple.asObservable();

	#selection = new UmbArrayState(<Array<string>>[]);
	public readonly selection = this.#selection.asObservable();

	repository?: UmbTreeRepository;

	#initResolver?: () => void;
	#initialized = false;

	#init = new Promise<void>((resolve) => {
		this.#initialized ? resolve() : (this.#initResolver = resolve);
	});

	constructor(host: UmbControllerHostElement, tree: ManifestTree) {
		this.host = host;
		this.tree = tree;

		const repositoryAlias = this.tree.meta.repositoryAlias;
		if (!repositoryAlias) throw new Error('Tree must have a repository alias.');

		new UmbObserverController(
			this.host,
			umbExtensionsRegistry.getByTypeAndAlias('repository', this.tree.meta.repositoryAlias),
			async (repositoryManifest) => {
				if (!repositoryManifest) return;

				try {
					const result = await createExtensionClass<UmbTreeRepository>(repositoryManifest, [this.host]);
					this.repository = result;
					this.#checkIfInitialized();
				} catch (error) {
					throw new Error('Could not create repository with alias: ' + repositoryAlias + '');
				}
			}
		);
	}

	// TODO: find a generic way to do this
	#checkIfInitialized() {
		if (this.repository) {
			this.#initialized = true;
			this.#initResolver?.();
		}
	}

	public setSelectable(value: boolean) {
		this.#selectable.next(value);
	}

	public getSelectable() {
		return this.#selectable.getValue();
	}

	public setMultiple(value: boolean) {
		this.#multiple.next(value);
	}

	public getMultiple() {
		return this.#multiple.getValue();
	}

	public setSelection(value: Array<string>) {
		if (!value) return;
		this.#selection.next(value);
	}

	public getSelection() {
		return this.#selection.getValue();
	}

	public select(id: string) {
		if (!this.getSelectable()) return;
		const newSelection = this.getMultiple() ? [...this.getSelection(), id] : [id];
		this.#selection.next(newSelection);
	}

	public deselect(id: string) {
		const newSelection = this.getSelection().filter((x) => x !== id);
		this.#selection.next(newSelection);
	}

	public async requestRootItems() {
		await this.#init;
		return this.repository!.requestRootTreeItems();
	}

	public async requestChildrenOf(parentId: string | null) {
		await this.#init;
		return this.repository!.requestTreeItemsOf(parentId);
	}

	public async rootItems() {
		await this.#init;
		return this.repository!.rootTreeItems();
	}

	public async childrenOf(parentId: string | null) {
		await this.#init;
		return this.repository!.treeItemsOf(parentId);
	}
}
