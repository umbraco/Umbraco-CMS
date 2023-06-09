import { Observable, map } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbPagedData, UmbTreeRepository } from '@umbraco-cms/backoffice/repository';
import { ManifestTree, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbBooleanState, UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { createExtensionClass } from '@umbraco-cms/backoffice/extension-api';
import { ProblemDetailsModel, TreeItemPresentationModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { UmbSelectionManagerBase } from '@umbraco-cms/backoffice/utils';

// TODO: update interface
export interface UmbTreeContext<TreeItemType extends TreeItemPresentationModel> {
	readonly selectable: Observable<boolean>;
	readonly selection: Observable<Array<string | null>>;
	setSelectable(value: boolean): void;
	getSelectable(): boolean;
	setMultiple(value: boolean): void;
	getMultiple(): boolean;
	setSelection(value: Array<string | null>): void;
	getSelection(): Array<string | null>;
	select(unique: string | null): void;
	deselect(unique: string | null): void;
	requestChildrenOf: (parentUnique: string | null) => Promise<{
		data?: UmbPagedData<TreeItemType>;
		error?: ProblemDetailsModel;
		asObservable?: () => Observable<TreeItemType[]>;
	}>;
}

export class UmbTreeContextBase<TreeItemType extends TreeItemPresentationModel>
	implements UmbTreeContext<TreeItemType>
{
	public host: UmbControllerHostElement;

	#selectionManager = new UmbSelectionManagerBase();

	#selectable = new UmbBooleanState(false);
	public readonly selectable = this.#selectable.asObservable();

	public readonly multiple = this.#selectionManager.multiple;
	public readonly selection = this.#selectionManager.selection;

	public repository?: UmbTreeRepository<TreeItemType>;
	public selectableFilter?: (item: TreeItemType) => boolean = () => true;

	#treeAlias?: string;

	#initResolver?: () => void;
	#initialized = false;

	#init = new Promise<void>((resolve) => {
		this.#initialized ? resolve() : (this.#initResolver = resolve);
	});

	constructor(host: UmbControllerHostElement) {
		this.host = host;
		new UmbContextProviderController(host, 'umbTreeContext', this);
	}

	// TODO: find a generic way to do this
	#checkIfInitialized() {
		if (this.repository) {
			this.#initialized = true;
			this.#initResolver?.();
		}
	}

	public async setTreeAlias(treeAlias?: string) {
		if (this.#treeAlias === treeAlias) return;
		this.#treeAlias = treeAlias;

		if (treeAlias) {
			this.#observeTreeManifest();
		}
	}

	public getTreeAlias() {
		return this.#treeAlias;
	}

	public setSelectable(value: boolean) {
		this.#selectable.next(value);
	}

	public getSelectable() {
		return this.#selectable.getValue();
	}

	public setMultiple(value: boolean) {
		this.#selectionManager.setMultiple(value);
	}

	public getMultiple() {
		return this.#selectionManager.getMultiple();
	}

	public setSelection(value: Array<string | null>) {
		this.#selectionManager.setSelection(value);
	}

	public getSelection() {
		return this.#selectionManager.getSelection();
	}

	public select(unique: string | null) {
		if (!this.getSelectable()) return;
		this.#selectionManager.select(unique);
		this.host.dispatchEvent(new CustomEvent('selected'));
	}

	public deselect(unique: string | null) {
		this.#selectionManager.deselect(unique);
		this.host.dispatchEvent(new CustomEvent('selected'));
	}

	public async requestTreeRoot() {
		await this.#init;
		return this.repository!.requestTreeRoot();
	}

	public async requestRootItems() {
		await this.#init;
		return this.repository!.requestRootTreeItems();
	}

	public async requestChildrenOf(parentUnique: string | null) {
		await this.#init;
		if (parentUnique === undefined) throw new Error('Parent unique cannot be undefined.');
		return this.repository!.requestTreeItemsOf(parentUnique);
	}

	public async rootItems() {
		await this.#init;
		return this.repository!.rootTreeItems();
	}

	public async childrenOf(parentUnique: string | null) {
		await this.#init;
		return this.repository!.treeItemsOf(parentUnique);
	}

	#observeTreeManifest() {
		new UmbObserverController<any>(
			this.host,
			umbExtensionsRegistry
				.extensionsOfType('tree')
				.pipe(map((treeManifests) => treeManifests.find((treeManifest) => treeManifest.alias === this.#treeAlias))),
			async (treeManifest) => {
				if (!treeManifest) return;
				this.#observeRepository(treeManifest);
			},
			'_observeTreeManifest'
		);
	}

	#observeRepository(treeManifest: ManifestTree) {
		const repositoryAlias = treeManifest.meta.repositoryAlias;
		if (!repositoryAlias) throw new Error('Tree must have a repository alias.');

		new UmbObserverController(
			this.host,
			umbExtensionsRegistry.getByTypeAndAlias('repository', treeManifest.meta.repositoryAlias),
			async (repositoryManifest) => {
				if (!repositoryManifest) return;

				try {
					const result = await createExtensionClass<UmbTreeRepository<TreeItemType>>(repositoryManifest, [this.host]);
					this.repository = result;
					this.#checkIfInitialized();
				} catch (error) {
					throw new Error('Could not create repository with alias: ' + repositoryAlias + '');
				}
			},
			'_observeRepository'
		);
	}
}
