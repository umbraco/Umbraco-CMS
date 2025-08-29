import type { UmbTreeItemModel, UmbTreeRootModel, UmbTreeStartNode } from '../types.js';
import type { UmbTreeRepository } from '../data/tree-repository.interface.js';
import type { UmbTreeContext } from '../tree-context.interface.js';
import type { UmbTreeRootItemsRequestArgs } from '../data/types.js';
import type { ManifestTree } from '../extensions/types.js';
import { UmbTreeExpansionManager } from '../expansion-manager/index.js';
import type { UmbTreeExpansionModel } from '../expansion-manager/types.js';
import { UMB_TREE_CONTEXT } from './default-tree.context-token.js';
import { type UmbActionEventContext, UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { type ManifestRepository, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import {
	UmbDeprecation,
	UmbPaginationManager,
	UmbSelectionManager,
	UmbTargetPaginationManager,
	debounce,
	type UmbOffsetPaginationRequestModel,
	type UmbTargetPaginationRequestModel,
} from '@umbraco-cms/backoffice/utils';
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
	extends UmbContextBase
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
	public readonly selection = new UmbSelectionManager(this);
	public readonly expansion = new UmbTreeExpansionManager(this);

	// Offset Pagination: TODO: deprecate and expose a new with the name "offsetPagination"
	// TODO: investigate if there is a way to combine the two paging types
	public readonly pagination = new UmbPaginationManager();

	// Target Pagination: Keeps track of pages when navigating with a target
	public readonly targetPagination = new UmbTargetPaginationManager(this);

	#hideTreeRoot = new UmbBooleanState(false);
	hideTreeRoot = this.#hideTreeRoot.asObservable();

	#expandTreeRoot = new UmbBooleanState(undefined);
	expandTreeRoot = this.#expandTreeRoot.asObservable();

	#startNode = new UmbObjectState<UmbTreeStartNode | undefined>(undefined);
	startNode = this.#startNode.asObservable();

	#foldersOnly = new UmbBooleanState(false);
	foldersOnly = this.#foldersOnly.asObservable();

	#manifest?: ManifestTree;
	#repository?: UmbTreeRepository<TreeItemType, TreeRootType>;
	#actionEventContext?: UmbActionEventContext;

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

		const take = 5;
		this.pagination.setPageSize(take);
		this.targetPagination.setTakeSize(take);

		this.#consumeContexts();

		// listen for page changes on the pagination manager
		this.pagination.addEventListener(UmbChangeEvent.TYPE, this.#onPageChange);

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
	public loadMore = (): Promise<void> => this.#loadNextItemsFromTarget();

	/**
	 * Load previous items of the tree item
	 * @memberof UmbTreeItemContextBase
	 * @returns {Promise<void>}
	 */
	public loadPrevItems = (): Promise<void> => this.#loadPrevItemsFromTarget();

	/**
	 * Load next items of the tree item
	 * @memberof UmbTreeItemContextBase
	 * @returns {Promise<void>}
	 */
	public loadNextItems = (): Promise<void> => this.#loadNextItemsFromTarget();

	#debouncedLoadTree(reload = false) {
		const hasStartNode = this.getStartNode();
		const hideTreeRoot = this.getHideTreeRoot();

		if (hasStartNode || hideTreeRoot) {
			this.#loadRootItems(reload);
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

	async #loadRootItems(reload = false) {
		await this.#init;

		// If we have a start node get children of that instead of the root
		const startNode = this.getStartNode();
		const foldersOnly = this.#foldersOnly.getValue();
		const additionalArgs = this.#additionalRequestArgs.getValue();
		const baseTarget = this.targetPagination.getBaseTarget();

		// When reloading we only want to send the target values with the request if we can find the target to reload from.
		const canSendTarget = reload === false || (reload && this.targetPagination.hasBaseTargetInCurrentItems());

		const targetPaging: UmbTargetPaginationRequestModel | undefined =
			baseTarget && baseTarget.unique && canSendTarget
				? {
						target: {
							unique: baseTarget.unique,
							entityType: baseTarget.entityType,
						},
						/* When we load from a target we want to load a few items before the target so the target isn't the first item in the list
						 Currently we use 5, but this could be anything that feels "right".
						 When reloading from target when want to retrieve the same number of items that a currently loaded
						*/
						takeBefore: reload ? this.targetPagination.getNumberOfCurrentItemsBeforeBaseTarget() : 5,
						takeAfter: reload
							? this.targetPagination.getNumberOfCurrentItemsAfterBaseTarget()
							: this.targetPagination.getTakeSize(),
					}
				: undefined;

		const offsetPaging: UmbOffsetPaginationRequestModel = {
			// when reloading we want to get everything from the start
			skip: reload ? 0 : this.pagination.getSkip(),
			take: reload
				? this.pagination.getCurrentPageNumber() * this.pagination.getPageSize()
				: this.pagination.getPageSize(),
		};

		const { data } = startNode?.unique
			? await this.#repository!.requestTreeItemsOf({
					parent: {
						unique: startNode.unique,
						entityType: startNode.entityType,
					},
					skip: offsetPaging.skip, // including this for backward compatibility
					take: offsetPaging.take, // including this for backward compatibility
					paging: targetPaging || offsetPaging,
					foldersOnly,
					...additionalArgs,
				})
			: await this.#repository!.requestTreeRootItems({
					skip: offsetPaging.skip, // including this for backward compatibility
					take: offsetPaging.take, // including this for backward compatibility
					paging: targetPaging || offsetPaging,
					foldersOnly,
					...additionalArgs,
				});

		if (data) {
			this.#rootItems.setValue(data.items);
			this.targetPagination.setCurrentItems(data.items);

			this.pagination.setTotalItems(data.total);
			this.targetPagination.setTotalItems(data.total);
			this.targetPagination.setTotalItemsBeforeStartTarget(data.totalBefore);
			this.targetPagination.setTotalItemsAfterEndTarget(data.totalAfter);
		}
	}

	async #loadPrevItemsFromTarget() {
		const startNode = this.getStartNode();
		const foldersOnly = this.#foldersOnly.getValue();
		const additionalArgs = this.#additionalRequestArgs.getValue();

		const targetPaging: UmbTargetPaginationRequestModel | undefined = {
			target: this.targetPagination.getStartTarget(),
			takeBefore: this.targetPagination.getTakeSize(),
			takeAfter: 0,
		};

		const { data } = startNode?.unique
			? await this.#repository!.requestTreeItemsOf({
					parent: {
						unique: startNode.unique,
						entityType: startNode.entityType,
					},
					paging: targetPaging,
					foldersOnly,
					...additionalArgs,
				})
			: await this.#repository!.requestTreeRootItems({
					paging: targetPaging,
					foldersOnly,
					...additionalArgs,
				});

		if (data) {
			// We have loaded previous items so we add them to the top of the array
			const reversedItems = [...data.items].reverse();
			this.#rootItems.prepend(reversedItems);
			this.targetPagination.prependCurrentItems(reversedItems);

			if (data.totalBefore === undefined) {
				throw new Error('totalBefore is missing in the response');
			}

			this.targetPagination.setTotalItems(data.total);
			this.targetPagination.setTotalItemsBeforeStartTarget(data.totalBefore);
		}
	}

	async #loadNextItemsFromTarget() {
		const startNode = this.getStartNode();
		const foldersOnly = this.#foldersOnly.getValue();
		const additionalArgs = this.#additionalRequestArgs.getValue();

		const targetPaging: UmbTargetPaginationRequestModel | undefined = {
			target: this.targetPagination.getEndTarget(),
			takeBefore: 0,
			takeAfter: this.targetPagination.getTakeSize(),
		};

		const { data } = startNode?.unique
			? await this.#repository!.requestTreeItemsOf({
					parent: {
						unique: startNode.unique,
						entityType: startNode.entityType,
					},
					paging: targetPaging,
					foldersOnly,
					...additionalArgs,
				})
			: await this.#repository!.requestTreeRootItems({
					paging: targetPaging,
					foldersOnly,
					...additionalArgs,
				});

		if (data) {
			this.#rootItems.append(data.items);
			this.targetPagination.appendCurrentItems(data.items);

			this.targetPagination.setTotalItems(data.total);
			this.targetPagination.setTotalItemsAfterEndTarget(data.totalAfter);
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
				const targetIsLoaded = this.#rootItems
					.getValue()
					.some((child) => child.entityType === newTarget?.entityType && newTarget.unique === child.unique);

				if (newTarget && targetIsLoaded) {
					return;
				}

				// If we already have children and the target didn't change then we don't have to load new children
				if (isExpanded && this.#rootItems.getValue().length > 0) {
					return;
				}

				if (isExpanded) {
					this.targetPagination.setBaseTarget(entry?.target);
					this.#loadRootItems();
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

	#resetTree() {
		this.#treeRoot.setValue(undefined);
		this.#rootItems.setValue([]);
		this.pagination.clear();
	}

	#consumeContexts() {
		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (instance) => {
			this.#removeEventListeners();
			this.#actionEventContext = instance;

			this.#actionEventContext?.addEventListener(
				UmbRequestReloadChildrenOfEntityEvent.TYPE,
				this.#onReloadRequest as EventListener,
			);
		});
	}

	#onPageChange = () => this.#loadNextItemsFromTarget();

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

	#onReloadRequest = (event: UmbEntityActionEvent) => {
		// Only handle root request here. Items are handled by the tree item context
		const treeRoot = this.#treeRoot.getValue();
		if (treeRoot === undefined) return;
		if (event.getUnique() !== treeRoot.unique) return;
		if (event.getEntityType() !== treeRoot.entityType) return;
		this.#loadRootItems(true);
	};

	#removeEventListeners() {
		this.#actionEventContext?.removeEventListener(
			UmbRequestReloadChildrenOfEntityEvent.TYPE,
			this.#onReloadRequest as EventListener,
		);
	}

	override destroy(): void {
		this.#removeEventListeners();
		super.destroy();
	}
}

export { UmbDefaultTreeContext as api };
