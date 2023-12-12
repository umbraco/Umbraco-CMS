import { type UmbTreeItemModelBase } from './types.js';
import { type UmbTreeRepository } from './tree-repository.interface.js';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbPagedData } from '@umbraco-cms/backoffice/repository';
import {
	type ManifestRepository,
	type ManifestTree,
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import { type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { ProblemDetails } from '@umbraco-cms/backoffice/backend-api';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import { UmbSelectionChangeEvent } from '@umbraco-cms/backoffice/event';

// TODO: update interface
export interface UmbTreeContext<TreeItemType extends UmbTreeItemModelBase> extends UmbBaseController {
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
		error?: ProblemDetails;
		asObservable?: () => Observable<TreeItemType[]>;
	}>;
}

export class UmbTreeContextBase<TreeItemType extends UmbTreeItemModelBase>
	extends UmbBaseController
	implements UmbTreeContext<TreeItemType>
{
	#selectionManager = new UmbSelectionManager();

	#selectable = new UmbBooleanState(false);
	public readonly selectable = this.#selectable.asObservable();

	public readonly multiple = this.#selectionManager.multiple;
	public readonly selection = this.#selectionManager.selection;

	public repository?: UmbTreeRepository<TreeItemType>;
	public selectableFilter?: (item: TreeItemType) => boolean = () => true;

	public filter?: (item: TreeItemType) => boolean = () => true;

	#treeAlias?: string;

	#initResolver?: () => void;
	#initialized = false;

	#init = new Promise<void>((resolve) => {
		this.#initialized ? resolve() : (this.#initResolver = resolve);
	});

	constructor(host: UmbControllerHostElement) {
		super(host);
		this.provideContext('umbTreeContext', this);
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

		this.#observeTreeManifest();
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
		this._host.getHostElement().dispatchEvent(new UmbSelectionChangeEvent());
	}

	public deselect(unique: string | null) {
		this.#selectionManager.deselect(unique);
		this._host.getHostElement().dispatchEvent(new UmbSelectionChangeEvent());
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
		if (this.#treeAlias) {
			this.observe(
				umbExtensionsRegistry.getByTypeAndAlias('tree', this.#treeAlias),
				async (treeManifest) => {
					if (!treeManifest) return;
					this.#observeRepository(treeManifest);
				},
				'_observeTreeManifest',
			);
		}
	}

	#observeRepository(treeManifest: ManifestTree) {
		const repositoryAlias = treeManifest.meta.repositoryAlias;
		if (!repositoryAlias) throw new Error('Tree must have a repository alias.');

		new UmbExtensionApiInitializer<ManifestRepository<UmbTreeRepository<TreeItemType>>>(
			this,
			umbExtensionsRegistry,
			repositoryAlias,
			[this._host],
			(permitted, ctrl) => {
				this.repository = permitted ? ctrl.api : undefined;
				this.#checkIfInitialized();
			},
		);
	}
}
