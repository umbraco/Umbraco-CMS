import type { UmbTreeItemModel, UmbTreeRootModel, UmbTreeStartNode } from '../types.js';
import type { UmbTreeContext } from '../tree-context.interface.js';
import type { UmbTreeRootItemsRequestArgs } from '../data/types.js';
import type { ManifestTree } from '../extensions/types.js';
import { UmbTreeExpansionManager } from '../expansion-manager/index.js';
import type { UmbTreeExpansionModel } from '../expansion-manager/types.js';
import { UmbTreeItemChildrenManager } from '../tree-item-children.manager.js';
import type { UmbTreeRepository } from '../data/index.js';
import { UMB_TREE_CONTEXT } from './default-tree.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDeprecation, UmbSelectionManager, debounce } from '@umbraco-cms/backoffice/utils';
import { UmbBooleanState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { umbExtensionsRegistry, type ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';

export class UmbDefaultTreeContext<
		TreeItemType extends UmbTreeItemModel,
		TreeRootType extends UmbTreeRootModel,
		RequestArgsType extends UmbTreeRootItemsRequestArgs = UmbTreeRootItemsRequestArgs,
	>
	extends UmbContextBase
	implements UmbTreeContext
{
	#treeItemChildrenManager = new UmbTreeItemChildrenManager<TreeItemType>(this);

	#treeRoot = new UmbObjectState<TreeRootType | undefined>(undefined);
	public readonly treeRoot = this.#treeRoot.asObservable();

	public readonly rootItems = this.#treeItemChildrenManager.children;

	public selectableFilter?: (item: TreeItemType) => boolean = () => true;
	public filter?: (item: TreeItemType) => boolean = () => true;
	public readonly selection = new UmbSelectionManager(this);
	public readonly expansion = new UmbTreeExpansionManager(this);

	// Offset Pagination: TODO: deprecate and expose a new with the name "offsetPagination"
	public readonly pagination = this.#treeItemChildrenManager.offsetPagination;

	// Target Pagination: Keeps track of pages when navigating with a target
	public readonly targetPagination = this.#treeItemChildrenManager.targetPagination;

	#hideTreeRoot = new UmbBooleanState(false);
	public readonly hideTreeRoot = this.#hideTreeRoot.asObservable();

	#expandTreeRoot = new UmbBooleanState(undefined);
	public readonly expandTreeRoot = this.#expandTreeRoot.asObservable();

	public readonly startNode = this.#treeItemChildrenManager.parent;
	public readonly foldersOnly = this.#treeItemChildrenManager.foldersOnly;
	public readonly additionalRequestArgs = this.#treeItemChildrenManager.additionalRequestArgs;

	#manifest?: ManifestTree;
	#repository?: UmbTreeRepository<TreeItemType, TreeRootType>;

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
		super(host, UMB_TREE_CONTEXT);
		this.#treeItemChildrenManager.setTakeSize(5);
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

	/**
	 * Returns the manifest.
	 * @returns {ManifestTree}
	 * @memberof UmbDefaultTreeContext
	 * @deprecated Use the `.manifest` property instead.
	 */
	public getManifest() {
		new UmbDeprecation({
			removeInVersion: '18.0.0',
			deprecated: 'getManifest',
			solution: 'Use .manifest property instead',
		}).warn();
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
	 * Reloads the tree in its current expansion state
	 * @memberof UmbDefaultTreeContext
	 */
	// TODO: debouncing the load tree method because multiple props can be set at the same time
	// that would trigger multiple loadTree calls. This is a temporary solution to avoid that.
	public reloadTree = debounce(() => this.#debouncedLoadTree(true), 100);

	/**
	 * Reloads the tree
	 * @memberof UmbDefaultTreeContext
	 * @returns {Promise<void>}
	 */
	public loadMore = (): Promise<void> => this.#treeItemChildrenManager.loadNextChildren();

	/**
	 * Load previous items of the tree item
	 * @memberof UmbTreeItemContextBase
	 * @returns {Promise<void>}
	 */
	public loadPrevItems = (): Promise<void> => this.#treeItemChildrenManager.loadPrevChildren();

	/**
	 * Load next items of the tree item
	 * @memberof UmbTreeItemContextBase
	 * @returns {Promise<void>}
	 */
	public loadNextItems = (): Promise<void> => this.#treeItemChildrenManager.loadNextChildren();

	#debouncedLoadTree(reload = false) {
		const hasStartNode = this.getStartNode();
		const hideTreeRoot = this.getHideTreeRoot();

		if (hasStartNode || hideTreeRoot) {
			this.#treeItemChildrenManager.reloadChildren();
		} else {
			this.#loadTreeRoot(reload);
		}
	}

	async #loadTreeRoot(reload = false) {
		await this.#init;

		const { data } = await this.#repository!.requestTreeRoot();

		if (data) {
			this.#treeRoot.setValue(data);
			this.pagination.setTotalItems(1);
			this.#observeExpansion();

			if (!reload) {
				if (this.getExpandTreeRoot()) {
					this.#toggleTreeRootExpansion(true);
				}
			}
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
		this.#clearTree();
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
		this.#treeItemChildrenManager.setParent(startNode);
		// we need to reset the tree if this config changes
		this.#clearTree();
		this.loadTree();
	}

	/**
	 * Gets the startNode config
	 * @returns {UmbTreeStartNode} - The start node
	 * @memberof UmbDefaultTreeContext
	 */
	getStartNode() {
		return this.#treeItemChildrenManager.getParent();
	}

	/**
	 * Sets the foldersOnly config
	 * @param {boolean} foldersOnly - Whether to show only folders
	 * @memberof UmbDefaultTreeContext
	 */
	setFoldersOnly(foldersOnly: boolean) {
		this.#treeItemChildrenManager.setFoldersOnly(foldersOnly);
		// we need to reset the tree if this config changes
		this.#clearTree();
		this.loadTree();
	}

	/**
	 * Gets the foldersOnly config
	 * @returns {boolean} - Whether to show only folders
	 * @memberof UmbDefaultTreeContext
	 */
	getFoldersOnly(): boolean {
		return this.#treeItemChildrenManager.getFoldersOnly();
	}

	/**
	 * Updates the requestArgs config and reloads the tree.
	 * @param args
	 */
	public updateAdditionalRequestArgs(args: Partial<RequestArgsType>) {
		this.#treeItemChildrenManager.setAdditionalRequestArgs(args);
		this.#clearTree();
		this.loadTree();
	}

	public getAdditionalRequestArgs() {
		return this.#treeItemChildrenManager.getAdditionalRequestArgs();
	}

	/**
	 * Sets the expansion state
	 * @param {UmbTreeExpansionModel} data - The expansion state
	 * @returns {void}
	 * @memberof UmbDefaultTreeContext
	 */
	setExpansion(data: UmbTreeExpansionModel): void {
		this.expansion.setExpansion(data);
	}

	/**
	 * Gets the expansion state
	 * @returns {UmbTreeExpansionModel} - The expansion state
	 * @memberof UmbDefaultTreeContext
	 */
	getExpansion(): UmbTreeExpansionModel {
		return this.expansion.getExpansion();
	}

	/**
	 * Sets the expandTreeRoot config
	 * @param {boolean} expandTreeRoot - Whether to expand the tree root
	 * @memberof UmbDefaultTreeContext
	 */
	setExpandTreeRoot(expandTreeRoot: boolean) {
		this.#expandTreeRoot.setValue(expandTreeRoot);
		this.#toggleTreeRootExpansion(expandTreeRoot);
	}

	/**
	 * Gets the expandTreeRoot config
	 * @returns {boolean | undefined} - Whether to expand the tree root
	 * @memberof UmbDefaultTreeContext
	 */
	getExpandTreeRoot(): boolean | undefined {
		return this.#expandTreeRoot.getValue();
	}

	#observeExpansion() {
		const entity = this.#treeRoot.getValue();
		if (!entity) return;

		this.observe(
			this.expansion.entry(entity),
			async (entry) => {
				const isExpanded = entry !== undefined;
				const currentBaseTarget = this.targetPagination.getBaseTarget();
				const newTarget = entry?.target;

				/* If a base target already exists (tree loaded to that point),
   			don’t auto-reset when the target is removed.
   			This happens when creating new items not yet in the tree. */
				if (currentBaseTarget && !newTarget) {
					return;
				}

				/* If a new target is set we only want to reload children if the new target isn’t among the already loaded items. */
				const targetIsLoaded = this.#treeItemChildrenManager.isChildLoaded(newTarget);
				if (newTarget && targetIsLoaded) {
					return;
				}

				// If we already have children and the target didn't change then we don't have to load new children
				if (isExpanded && this.#treeItemChildrenManager.hasLoadedChildren()) {
					return;
				}

				if (isExpanded) {
					this.targetPagination.setBaseTarget(entry?.target);
					this.#treeItemChildrenManager.loadChildren();
				}
			},
			'observeExpansion',
		);
	}

	#toggleTreeRootExpansion(expand: boolean) {
		const treeRoot = this.#treeRoot.getValue();
		if (!treeRoot) return;
		const treeRootEntity = { entityType: treeRoot.entityType, unique: treeRoot.unique };

		if (expand) {
			this.expansion.expandItem(treeRootEntity);
		} else {
			this.expansion.collapseItem(treeRootEntity);
		}
	}

	#clearTree() {
		this.#treeRoot.setValue(undefined);
		this.#treeItemChildrenManager.clear();
	}

	#observeRepository(repositoryAlias?: string) {
		if (!repositoryAlias) throw new Error('Tree must have a repository alias.');

		new UmbExtensionApiInitializer<ManifestRepository<UmbTreeRepository<TreeItemType>>>(
			this,
			umbExtensionsRegistry,
			repositoryAlias,
			[this],
			(permitted, ctrl) => {
				this.#repository = permitted ? ctrl.api : undefined;
				this.#checkIfInitialized();
			},
		);
	}
}

export { UmbDefaultTreeContext as api };
