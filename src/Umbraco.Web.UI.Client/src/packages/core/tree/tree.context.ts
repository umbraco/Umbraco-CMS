import { UmbReloadTreeItemChildrenRequestEntityActionEvent } from './reload-tree-item-children/index.js';
import type { UmbTreeItemModelBase } from './types.js';
import type { UmbTreeRepository } from './data/tree-repository.interface.js';
import { type UmbActionEventContext, UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbPagedModel } from '@umbraco-cms/backoffice/repository';
import {
	type ManifestRepository,
	type ManifestTree,
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import type { ProblemDetails } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import type { UmbEntityActionEvent } from '@umbraco-cms/backoffice/entity-action';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

// TODO: update interface
export interface UmbTreeContext<TreeItemType extends UmbTreeItemModelBase> extends UmbBaseController {
	selection: UmbSelectionManager;
	requestChildrenOf: (parentUnique: string | null) => Promise<{
		data?: UmbPagedModel<TreeItemType>;
		error?: ProblemDetails;
		asObservable?: () => Observable<TreeItemType[]>;
	}>;
}

export class UmbTreeContextBase<TreeItemType extends UmbTreeItemModelBase>
	extends UmbBaseController
	implements UmbTreeContext<TreeItemType>
{
	#treeRoot = new UmbObjectState<TreeItemType | undefined>(undefined);
	treeRoot = this.#treeRoot.asObservable();

	public repository?: UmbTreeRepository<TreeItemType>;
	public selectableFilter?: (item: TreeItemType) => boolean = () => true;
	public filter?: (item: TreeItemType) => boolean = () => true;
	public readonly selection = new UmbSelectionManager(this._host);

	#treeAlias?: string;
	#actionEventContext?: UmbActionEventContext;

	#initResolver?: () => void;
	#initialized = false;

	#init = new Promise<void>((resolve) => {
		this.#initialized ? resolve() : (this.#initResolver = resolve);
	});

	constructor(host: UmbControllerHostElement) {
		super(host);
		this.provideContext('umbTreeContext', this);

		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (instance) => {
			this.#actionEventContext = instance;
			this.#actionEventContext.removeEventListener(
				UmbReloadTreeItemChildrenRequestEntityActionEvent.TYPE,
				this.#onReloadRequest as EventListener,
			);
			this.#actionEventContext.addEventListener(
				UmbReloadTreeItemChildrenRequestEntityActionEvent.TYPE,
				this.#onReloadRequest as EventListener,
			);
		});

		this.requestTreeRoot();
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
		const { data } = await this.repository!.requestTreeRoot();

		if (data) {
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			this.#treeRoot.setValue(data);
		}
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
				umbExtensionsRegistry.byTypeAndAlias('tree', this.#treeAlias),
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

	#onReloadRequest = (event: UmbEntityActionEvent) => {
		// Only handle root request here. Items are handled by the tree item context
		const treeRoot = this.#treeRoot.getValue();
		if (treeRoot === undefined) return;
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		if (event.getUnique() !== treeRoot.unique) return;
		if (event.getEntityType() !== treeRoot.entityType) return;
		this.requestRootItems();
	};

	destroy(): void {
		this.#actionEventContext?.removeEventListener(
			UmbReloadTreeItemChildrenRequestEntityActionEvent.TYPE,
			this.#onReloadRequest as EventListener,
		);
		super.destroy();
	}
}
