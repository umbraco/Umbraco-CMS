import type { UmbTreeItemModel, UmbTreeRootModel, UmbTreeStartNode } from '../types.js';
import type { UmbTreeRepository } from '../data/tree-repository.interface.js';
import type { UmbTreeContext } from '../tree-context.interface.js';
import type { UmbTreeRootItemsRequestArgs } from '../data/types.js';
import type { ManifestTree } from '../extensions/index.js';
import { UMB_TREE_CONTEXT } from './default-tree.context-token.js';
import { type UmbActionEventContext, UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { type ManifestRepository, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbPaginationManager, UmbSelectionManager, debounce } from '@umbraco-cms/backoffice/utils';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	type UmbEntityActionEvent,
} from '@umbraco-cms/backoffice/entity-action';
import { UmbArrayState, UmbBooleanState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

export class UmbDefaultTreeContext<
		TreeItemType extends UmbTreeItemModel,
		TreeRootType extends UmbTreeRootModel,
		RequestArgsType extends UmbTreeRootItemsRequestArgs = UmbTreeRootItemsRequestArgs,
	>
	extends UmbContextBase<UmbDefaultTreeContext<TreeItemType, TreeRootType, RequestArgsType>>
	implements UmbTreeContext
{
	#additionalRequestArgs = new UmbObjectState<Partial<RequestArgsType> | object>({});
	public readonly additionalRequestArgs = this.#additionalRequestArgs.asObservable();

	#treeRoot = new UmbObjectState<TreeRootType | undefined>(undefined);
	treeRoot = this.#treeRoot.asObservable();

	#rootItems = new UmbArrayState<TreeItemType>([], (x) => x.unique);
	rootItems = this.#rootItems.asObservable();

	public selectableFilter?: (item: TreeItemType) => boolean = () => true;
	public filter?: (item: TreeItemType) => boolean = () => true;
	public readonly selection = new UmbSelectionManager(this._host);
	public readonly pagination = new UmbPaginationManager();

	#hideTreeRoot = new UmbBooleanState(false);
	hideTreeRoot = this.#hideTreeRoot.asObservable();

	#startNode = new UmbObjectState<UmbTreeStartNode | undefined>(undefined);
	startNode = this.#startNode.asObservable();

	#foldersOnly = new UmbBooleanState(false);
	foldersOnly = this.#foldersOnly.asObservable();

	#manifest?: ManifestTree;
	#repository?: UmbTreeRepository<TreeItemType, TreeRootType>;
	#actionEventContext?: UmbActionEventContext;

	#paging = {
		skip: 0,
		take: 50,
	};

	#initResolver?: () => void;
	#initialized = false;

	#init = new Promise<void>((resolve) => {
		if (this.#initialized) {
			resolve();
		} else {
			this.#initResolver = resolve;
		}
	});

	constructor(host: UmbControllerHost) {
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		super(host, UMB_TREE_CONTEXT);
		this.pagination.setPageSize(this.#paging.take);
		this.#consumeContexts();

		// listen for page changes on the pagination manager
		this.pagination.addEventListener(UmbChangeEvent.TYPE, this.#onPageChange);

		/* TODO: revisit. This is a temp solution to notify the parent it needs to reload its children
		there might be a better way to do this through a tree item parent context.
		It does not look like there is a way to have a "dynamic" parent context that will stop when a
		specific parent is reached (a tree item unique that matches the parentUnique of this item) */
		const hostElement = this.getHostElement();
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		hostElement.addEventListener('temp-reload-tree-item-parent', (event: CustomEvent) => {
			const treeRoot = this.#treeRoot.getValue();
			const unique = treeRoot?.unique;

			if (event.detail.unique === unique) {
				event.stopPropagation();
				this.loadTree();
			}
		});

		// always load the tree root because we need the root entity to reload the entire tree
		this.#loadTreeRoot();
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
	 * @returns {ManifestTree}
	 * @memberof UmbDefaultTreeContext
	 */
	public getManifest() {
		return this.#manifest;
	}

	public getRepository() {
		return this.#repository;
	}

	/**
	 * Loads the tree
	 * @memberof UmbDefaultTreeContext
	 */
	// TODO: debouncing the load tree method because multiple props can be set at the same time
	// that would trigger multiple loadTree calls. This is a temporary solution to avoid that.
	public loadTree = debounce(() => this.#debouncedLoadTree(), 100);

	/**
	 * Reloads the tree
	 * @memberof UmbDefaultTreeContext
	 * @returns {void}
	 */
	public loadMore = () => this.#debouncedLoadTree(true);

	#debouncedLoadTree(reload = false) {
		if (this.getStartNode()) {
			this.#loadRootItems(reload);
			return;
		}

		const hideTreeRoot = this.getHideTreeRoot();
		if (hideTreeRoot) {
			this.#loadRootItems(reload);
			return;
		}
	}

	async #loadTreeRoot() {
		await this.#init;

		const { data } = await this.#repository!.requestTreeRoot();

		if (data) {
			this.#treeRoot.setValue(data);
			this.pagination.setTotalItems(1);
		}
	}

	async #loadRootItems(loadMore = false) {
		await this.#init;

		const skip = loadMore ? this.#paging.skip : 0;
		const take = loadMore ? this.#paging.take : this.pagination.getCurrentPageNumber() * this.#paging.take;

		// If we have a start node get children of that instead of the root
		const startNode = this.getStartNode();
		const foldersOnly = this.#foldersOnly.getValue();
		const additionalArgs = this.#additionalRequestArgs.getValue();

		const { data } = startNode?.unique
			? await this.#repository!.requestTreeItemsOf({
					...additionalArgs,
					parent: {
						unique: startNode.unique,
						entityType: startNode.entityType,
					},
					foldersOnly,
					skip,
					take,
				})
			: await this.#repository!.requestTreeRootItems({
					...additionalArgs,
					foldersOnly,
					skip,
					take,
				});

		if (data) {
			if (loadMore) {
				const currentItems = this.#rootItems.getValue();
				this.#rootItems.setValue([...currentItems, ...data.items]);
			} else {
				this.#rootItems.setValue(data.items);
			}

			this.pagination.setTotalItems(data.total);
		}
	}

	/**
	 * Sets the hideTreeRoot config
	 * @param {boolean} hideTreeRoot - Whether to hide the tree root
	 * @memberof UmbDefaultTreeContext
	 */
	setHideTreeRoot(hideTreeRoot: boolean) {
		this.#hideTreeRoot.setValue(hideTreeRoot);
		// we need to reset the tree if this config changes
		this.#resetTree();
		this.loadTree();
	}

	/**
	 * Gets the hideTreeRoot config
	 * @returns {boolean}
	 * @memberof UmbDefaultTreeContext
	 */
	getHideTreeRoot() {
		return this.#hideTreeRoot.getValue();
	}

	/**
	 * Sets the startNode config
	 * @param {UmbTreeStartNode} startNode
	 * @memberof UmbDefaultTreeContext
	 */
	setStartNode(startNode: UmbTreeStartNode | undefined) {
		this.#startNode.setValue(startNode);
		// we need to reset the tree if this config changes
		this.#resetTree();
		this.loadTree();
	}

	/**
	 * Gets the startNode config
	 * @returns {UmbTreeStartNode} - The start node
	 * @memberof UmbDefaultTreeContext
	 */
	getStartNode() {
		return this.#startNode.getValue();
	}

	/**
	 * Sets the foldersOnly config
	 * @param {boolean} foldersOnly - Whether to show only folders
	 * @memberof UmbDefaultTreeContext
	 */
	setFoldersOnly(foldersOnly: boolean) {
		this.#foldersOnly.setValue(foldersOnly);
		// we need to reset the tree if this config changes
		this.#resetTree();
		this.loadTree();
	}

	/**
	 * Gets the foldersOnly config
	 * @returns {boolean} - Whether to show only folders
	 * @memberof UmbDefaultTreeContext
	 */
	getFoldersOnly() {
		return this.#foldersOnly.getValue();
	}

	/**
	 * Updates the requestArgs config and reloads the tree.
	 * @param args
	 */
	public updateAdditionalRequestArgs(args: Partial<RequestArgsType>) {
		this.#additionalRequestArgs.setValue({ ...this.#additionalRequestArgs.getValue(), ...args });
		this.#resetTree();
		this.loadTree();
	}

	public getAdditionalRequestArgs() {
		return this.#additionalRequestArgs.getValue();
	}

	#resetTree() {
		this.#treeRoot.setValue(undefined);
		this.#rootItems.setValue([]);
		this.pagination.clear();
	}

	#consumeContexts() {
		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (instance) => {
			this.#actionEventContext = instance;

			this.#actionEventContext.removeEventListener(
				UmbRequestReloadChildrenOfEntityEvent.TYPE,
				this.#onReloadRequest as EventListener,
			);

			this.#actionEventContext.removeEventListener(
				UmbRequestReloadChildrenOfEntityEvent.TYPE,
				this.#onReloadRequest as EventListener,
			);

			this.#actionEventContext.addEventListener(
				UmbRequestReloadChildrenOfEntityEvent.TYPE,
				this.#onReloadRequest as EventListener,
			);

			this.#actionEventContext.addEventListener(
				UmbRequestReloadChildrenOfEntityEvent.TYPE,
				this.#onReloadRequest as EventListener,
			);
		});
	}

	#onPageChange = (event: UmbChangeEvent) => {
		const target = event.target as UmbPaginationManager;
		this.#paging.skip = target.getSkip();
		this.loadMore();
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
		if (event.getUnique() !== treeRoot.unique) return;
		if (event.getEntityType() !== treeRoot.entityType) return;
		this.loadTree();
	};

	override destroy(): void {
		this.#actionEventContext?.removeEventListener(
			UmbRequestReloadChildrenOfEntityEvent.TYPE,
			this.#onReloadRequest as EventListener,
		);

		this.#actionEventContext?.removeEventListener(
			UmbRequestReloadChildrenOfEntityEvent.TYPE,
			this.#onReloadRequest as EventListener,
		);

		super.destroy();
	}
}

export { UmbDefaultTreeContext as api };
