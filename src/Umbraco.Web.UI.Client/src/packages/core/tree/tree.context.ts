import { type UmbTreeItemModelBase } from './types.js';
import { type UmbTreeRepository } from './tree-repository.interface.js';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbPagedData } from '@umbraco-cms/backoffice/repository';
import {
	type ManifestRepository,
	type ManifestTree,
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import { type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { ProblemDetails } from '@umbraco-cms/backoffice/backend-api';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';

// TODO: update interface
export interface UmbTreeContext<TreeItemType extends UmbTreeItemModelBase> extends UmbBaseController {
	selection: UmbSelectionManager;
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
	public repository?: UmbTreeRepository<TreeItemType>;
	public selectableFilter?: (item: TreeItemType) => boolean = () => true;

	public readonly selection = new UmbSelectionManager(this._host);

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
