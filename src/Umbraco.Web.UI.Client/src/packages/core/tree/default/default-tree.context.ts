import { UmbReloadTreeItemChildrenRequestEntityActionEvent } from '../reload-tree-item-children/index.js';
import type { UmbTreeItemModelBase } from '../types.js';
import type { UmbTreeRepository } from '../data/tree-repository.interface.js';
import type { UmbTreeContext } from '../tree-context.interface.js';
import { type UmbActionEventContext, UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import {
	type ManifestRepository,
	type ManifestTree,
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbPaginationManager, UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import type { UmbEntityActionEvent } from '@umbraco-cms/backoffice/entity-action';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

export class UmbDefaultTreeContext<TreeItemType extends UmbTreeItemModelBase>
	extends UmbContextBase<UmbDefaultTreeContext<TreeItemType>>
	implements UmbTreeContext
{
	#treeRoot = new UmbObjectState<TreeItemType | undefined>(undefined);
	treeRoot = this.#treeRoot.asObservable();

	public selectableFilter?: (item: TreeItemType) => boolean = () => true;
	public filter?: (item: TreeItemType) => boolean = () => true;
	public readonly selection = new UmbSelectionManager(this._host);
	public readonly pagination = new UmbPaginationManager();

	#manifest?: ManifestTree;
	#repository?: UmbTreeRepository<TreeItemType>;
	#actionEventContext?: UmbActionEventContext;

	#paging = {
		skip: 0,
		take: 50,
	};

	#initResolver?: () => void;
	#initialized = false;

	#init = new Promise<void>((resolve) => {
		this.#initialized ? resolve() : (this.#initResolver = resolve);
	});

	constructor(host: UmbControllerHost) {
		super(host, UMB_DEFAULT_TREE_CONTEXT);
		this.pagination.setPageSize(this.#paging.take);
		this.#consumeContexts();

		// listen for page changes on the pagination manager
		this.pagination.addEventListener(UmbChangeEvent.TYPE, this.#onPageChange);
		this.requestTreeRoot();
	}

	// TODO: find a generic way to do this
	#checkIfInitialized() {
		if (this.#repository) {
			this.#initialized = true;
			this.#initResolver?.();
		}
	}

	/**
	 * Sets the manifest
	 * @param {ManifestTree} manifest
	 * @memberof UmbDefaultTreeContext
	 */
	public set manifest(manifest: ManifestTree | undefined) {
		if (this.#manifest === manifest) return;
		this.#manifest = manifest;
		this.#observeRepository(this.#manifest?.meta.repositoryAlias);
	}
	public get manifest() {
		return this.#manifest;
	}

	// TODO: getManifest, could be refactored to use the getter method [NL]
	/**
	 * Returns the manifest.
	 * @return {ManifestTree}
	 * @memberof UmbDefaultTreeContext
	 */
	public getManifest() {
		return this.#manifest;
	}

	public getRepository() {
		return this.#repository;
	}

	public async requestTreeRoot() {
		await this.#init;
		const { data } = await this.#repository!.requestTreeRoot();

		if (data) {
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			//@ts-ignore
			this.#treeRoot.setValue(data);
		}
	}

	public async requestRootItems() {
		await this.#init;

		const { data, error, asObservable } = await this.#repository!.requestRootTreeItems({
			skip: this.#paging.skip,
			take: this.#paging.take,
		});

		if (data) {
			this.pagination.setTotalItems(data.total);
		}

		return { data, error, asObservable };
	}

	public async rootItems() {
		await this.#init;
		return this.#repository!.rootTreeItems();
	}

	#consumeContexts() {
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
	}

	#onPageChange = (event: UmbChangeEvent) => {
		const target = event.target as UmbPaginationManager;
		this.#paging.skip = target.getSkip();
		this.requestRootItems();
	};

	#observeRepository(repositoryAlias?: string) {
		if (!repositoryAlias) throw new Error('Tree must have a repository alias.');

		new UmbExtensionApiInitializer<ManifestRepository<UmbTreeRepository<TreeItemType>>>(
			this,
			umbExtensionsRegistry,
			repositoryAlias,
			[this._host],
			(permitted, ctrl) => {
				this.#repository = permitted ? ctrl.api : undefined;
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

export default UmbDefaultTreeContext;

export const UMB_DEFAULT_TREE_CONTEXT = new UmbContextToken<UmbDefaultTreeContext<any>>('UmbTreeContext');
