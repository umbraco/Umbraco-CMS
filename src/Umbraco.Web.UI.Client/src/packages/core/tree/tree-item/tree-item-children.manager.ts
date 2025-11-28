import type { UmbTreeRootItemsRequestArgs } from '../data/index.js';
import type { UmbTreeItemModel, UmbTreeRootModel, UmbTreeStartNode } from '../types.js';
import { UMB_TREE_CONTEXT } from '../tree.context.token.js';
import { UmbRequestReloadTreeItemChildrenEvent } from '../entity-actions/reload-tree-item-children/index.js';
import { UMB_TREE_ITEM_CONTEXT } from './tree-item.context.token.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbArrayState, UmbBooleanState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import {
	UmbPaginationManager,
	UmbTargetPaginationManager,
	type UmbOffsetPaginationRequestModel,
	type UmbTargetPaginationRequestModel,
} from '@umbraco-cms/backoffice/utils';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import type { UmbEntityActionEvent } from '@umbraco-cms/backoffice/entity-action';
import {
	UmbHasChildrenEntityContext,
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';

type ResetReason = 'error' | 'empty' | 'fallback';

export class UmbTreeItemChildrenManager<
	TreeItemType extends UmbTreeItemModel = UmbTreeItemModel,
	TreeRootType extends UmbTreeRootModel = UmbTreeRootModel,
	RequestArgsType extends UmbTreeRootItemsRequestArgs = UmbTreeRootItemsRequestArgs,
> extends UmbControllerBase {
	public readonly offsetPagination = new UmbPaginationManager();
	public readonly targetPagination = new UmbTargetPaginationManager(this);

	#children = new UmbArrayState<TreeItemType>([], (x) => x.unique);
	public readonly children = this.#children.asObservable();

	#hasChildren = new UmbBooleanState(false);
	public readonly hasChildren = this.#hasChildren.asObservable();
	#hasChildrenContext = new UmbHasChildrenEntityContext(this);

	#treeItem = new UmbObjectState<TreeItemType | TreeRootType | undefined>(undefined);
	public readonly treeItem = this.#treeItem.asObservable();

	#foldersOnly = new UmbBooleanState(false);
	public readonly foldersOnly = this.#foldersOnly.asObservable();

	#additionalRequestArgs = new UmbObjectState<Partial<RequestArgsType> | object>({});
	public readonly additionalRequestArgs = this.#additionalRequestArgs.asObservable();

	#startNode = new UmbObjectState<UmbTreeStartNode | undefined>(undefined);
	public readonly startNode = this.#startNode.asObservable();

	#isLoading = new UmbBooleanState(false);
	public readonly isLoading = this.#isLoading.asObservable();

	#isLoadingPrevChildren = new UmbBooleanState(false);
	public readonly isLoadingPrevChildren = this.#isLoadingPrevChildren.asObservable();

	#isLoadingNextChildren = new UmbBooleanState(false);
	public readonly isLoadingNextChildren = this.#isLoadingNextChildren.asObservable();

	#takeSize: number = 50;
	#takeBeforeTarget?: number;
	#takeAfterTarget?: number;

	#actionEventContext?: typeof UMB_ACTION_EVENT_CONTEXT.TYPE;

	#treeContext?: typeof UMB_TREE_CONTEXT.TYPE;
	#parentTreeItemContext?: typeof UMB_TREE_ITEM_CONTEXT.TYPE;
	#requestMaxRetries = 2;

	constructor(host: UmbControllerHost) {
		super(host);
		// listen for page changes on the pagination manager
		this.offsetPagination.addEventListener(UmbChangeEvent.TYPE, this.#onPageChange);

		this.#listenForActionEvents();

		this.consumeContext(UMB_TREE_CONTEXT, (treeContext) => {
			this.#treeContext = treeContext;
		});

		this.consumeContext(UMB_TREE_ITEM_CONTEXT, (instance) => {
			this.#parentTreeItemContext = instance;
		}).skipHost();
	}

	public setTakeSize(size: number): void {
		this.#takeSize = size;
		this.offsetPagination.setPageSize(size);
		this.targetPagination.setTakeSize(size);
	}

	public setTargetTakeSize(before: number | undefined, after: number | undefined): void {
		this.#takeBeforeTarget = before;
		this.#takeAfterTarget = after;
	}

	public getTakeSize(): number {
		return this.#takeSize;
	}

	public setFoldersOnly(foldersOnly: boolean) {
		this.#foldersOnly.setValue(foldersOnly);
	}

	public getFoldersOnly() {
		return this.#foldersOnly.getValue();
	}

	/**
	 * Set the parent for which to load children.
	 * @param {(TreeItemType | TreeRootType | undefined)} treeItem - The tree item model
	 * @memberof UmbTreeItemChildrenManager
	 */
	public setTreeItem(treeItem: TreeItemType | TreeRootType | undefined) {
		this.#treeItem.setValue(treeItem);
		this.setHasChildren(treeItem?.hasChildren || false);
	}

	/**
	 * Gets the tree item for which to load children.
	 * @returns {TreeItemType | TreeRootType | undefined} - The tree item for the children
	 * @memberof UmbTreeItemChildrenManager
	 */
	public getTreeItem(): TreeItemType | TreeRootType | undefined {
		return this.#treeItem.getValue();
	}

	/**
	 * Sets additional request arguments that will be passed with the request.
	 * @param {Partial<RequestArgsType>} args - The additional request arguments
	 * @memberof UmbTreeItemChildrenManager
	 */
	public setAdditionalRequestArgs(args: Partial<RequestArgsType>) {
		this.#additionalRequestArgs.setValue({ ...this.#additionalRequestArgs.getValue(), ...args });
	}

	/**
	 * Gets additional request arguments that will be passed with the request.
	 * @returns {Partial<RequestArgsType> | undefined} - The additional request arguments.
	 * @memberof UmbTreeItemChildrenManager
	 */
	public getAdditionalRequestArgs(): Partial<RequestArgsType> | undefined {
		return this.#additionalRequestArgs.getValue();
	}

	/**
	 * Sets the startNode config
	 * @param {(UmbTreeStartNode | undefined)} startNode - The start node
	 * @memberof UmbTreeItemChildrenManager
	 */
	public setStartNode(startNode: UmbTreeStartNode | undefined) {
		this.#startNode.setValue(startNode);
	}

	/**
	 * Gets the startNode config
	 * @returns {(UmbTreeStartNode | undefined)} - The start node
	 * @memberof UmbTreeItemChildrenManager
	 */
	public getStartNode(): UmbTreeStartNode | undefined {
		return this.#startNode.getValue();
	}

	/**
	 * Sets the hasChildren state
	 * @param {boolean} hasChildren
	 * @memberof UmbTreeItemChildrenManager
	 */
	public setHasChildren(hasChildren: boolean) {
		this.#hasChildren.setValue(hasChildren);
		this.#hasChildrenContext.setHasChildren(hasChildren);
	}

	/**
	 * Gets the hasChildren state
	 * @returns {boolean} - True if the current tree item has children
	 * @memberof UmbTreeItemChildrenManager
	 */
	public getHasChildren(): boolean {
		return this.#hasChildren.getValue();
	}

	/**
	 * Loads the children for the current parent.
	 * @returns {Promise<void>}
	 * @memberof UmbTreeItemChildrenManager
	 */
	public async loadChildren(): Promise<void> {
		const target = this.targetPagination.getBaseTarget();
		/* If a new target is set we only want to reload children if the new target isn’t among the already loaded items. */
		if (target && this.isChildLoaded(target)) {
			return;
		}

		return this.#loadChildren();
	}

	/**
	 * Reloads the children for the current parent.
	 * @returns {Promise<void>}
	 * @memberof UmbTreeItemChildrenManager
	 */
	public async reloadChildren(): Promise<void> {
		this.#loadChildren(true);
	}

	public async loadPrevChildren(): Promise<void> {
		return this.#loadPrevItemsFromTarget();
	}

	public async loadNextChildren(): Promise<void> {
		return this.#loadNextItemsFromTarget();
	}

	#loadChildrenRetries = 0;
	async #loadChildren(reload = false) {
		if (this.#loadChildrenRetries > this.#requestMaxRetries) {
			this.#loadChildrenRetries = 0;
			this.#resetChildren('error');
			return;
		}

		const repository = this.#treeContext?.getRepository();
		if (!repository) throw new Error('Could not request children, repository is missing');

		this.#isLoading.setValue(true);

		const parent = this.getStartNode() || this.getTreeItem();
		const foldersOnly = this.getFoldersOnly();
		const additionalArgs = this.getAdditionalRequestArgs();
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
						takeBefore: reload
							? this.targetPagination.getNumberOfCurrentItemsBeforeBaseTarget()
							: this.#takeBeforeTarget !== undefined
								? this.#takeBeforeTarget
								: this.targetPagination.getTakeSize(),
						takeAfter: reload
							? this.targetPagination.getNumberOfCurrentItemsAfterBaseTarget()
							: this.#takeAfterTarget !== undefined
								? this.#takeAfterTarget
								: this.targetPagination.getTakeSize(),
					}
				: undefined;

		const offsetPaging: UmbOffsetPaginationRequestModel = {
			// when reloading we want to get everything from the start
			skip: reload ? 0 : this.offsetPagination.getSkip(),
			take: reload
				? this.offsetPagination.getCurrentPageNumber() * this.offsetPagination.getPageSize()
				: this.offsetPagination.getPageSize(),
		};

		const { data, error } = parent?.unique
			? await repository.requestTreeItemsOf({
					parent: {
						unique: parent.unique,
						entityType: parent.entityType,
					},
					skip: offsetPaging.skip, // including this for backward compatibility
					take: offsetPaging.take, // including this for backward compatibility
					paging: targetPaging || offsetPaging,
					foldersOnly,
					...additionalArgs,
				})
			: await repository.requestTreeRootItems({
					skip: offsetPaging.skip, // including this for backward compatibility
					take: offsetPaging.take, // including this for backward compatibility
					paging: targetPaging || offsetPaging,
					foldersOnly,
					...additionalArgs,
				});

		// We have used a baseTarget that no longer exists on the sever. We need to retry with a new target
		if (error && error.message.includes('not found')) {
			this.#loadChildrenRetries++;
			const newBaseTarget = this.targetPagination.getNewBaseTarget();
			this.targetPagination.removeFromCurrentItems(baseTarget!);

			if (newBaseTarget) {
				this.targetPagination.setBaseTarget(newBaseTarget);
				this.#loadChildren();
			} else {
				/*
					If we can't find a new base target we reload the children from the top.
					We cancel the base target and load using skip/take pagination instead.
					This can happen if deep linked to a non existing item or all retries have failed.
				*/
				this.#resetChildren(this.#children.getValue().length === 0 ? 'empty' : 'fallback');
			}
		}

		if (data) {
			const items = data.items as Array<TreeItemType>;
			this.#children.setValue(items);
			this.setHasChildren(data.total > 0);

			this.offsetPagination.setTotalItems(data.total);

			this.targetPagination.setCurrentItems(items);
			this.targetPagination.setTotalItems(data.total);
			this.targetPagination.setTotalItemsBeforeStartTarget(data.totalBefore);
			this.targetPagination.setTotalItemsAfterEndTarget(data.totalAfter);

			this.#loadChildrenRetries = 0;
		}

		this.#isLoading.setValue(false);
	}

	#loadPrevItemsRetries = 0;
	async #loadPrevItemsFromTarget() {
		if (this.#loadPrevItemsRetries > this.#requestMaxRetries) {
			// If we have exceeded the maximum number of retries, we need to reset the base target and load from the top
			this.#loadPrevItemsRetries = 0;
			this.#resetChildren('error');
			return;
		}

		const repository = this.#treeContext?.getRepository();
		if (!repository) throw new Error('Could not request children, repository is missing');

		this.#isLoading.setValue(true);
		this.#isLoadingPrevChildren.setValue(true);

		const parent = this.getStartNode() || this.getTreeItem();
		const foldersOnly = this.getFoldersOnly();
		const additionalArgs = this.getAdditionalRequestArgs();
		const startTarget = this.targetPagination.getStartTarget();

		const targetPaging: UmbTargetPaginationRequestModel | undefined = {
			target: startTarget,
			takeBefore: this.targetPagination.getTakeSize(),
			takeAfter: 0,
		};

		const { data, error } = parent?.unique
			? await repository.requestTreeItemsOf({
					parent: {
						unique: parent.unique,
						entityType: parent.entityType,
					},
					paging: targetPaging,
					foldersOnly,
					...additionalArgs,
				})
			: await repository.requestTreeRootItems({
					paging: targetPaging,
					foldersOnly,
					...additionalArgs,
				});

		if (error && error.message.includes('not found')) {
			this.#loadPrevItemsRetries++;
			const newStartTarget = this.targetPagination.getNewStartTarget();
			this.targetPagination.removeFromCurrentItems(startTarget);

			if (newStartTarget) {
				this.#loadPrevItemsFromTarget();
			} else {
				/*
					If we can't find a new end target we reload the children from the top.
					We cancel the base target and load using skip/take pagination instead.
				*/
				this.#resetChildren(this.#children.getValue().length === 0 ? 'empty' : 'fallback');
			}
		}

		if (data) {
			if (data.totalBefore === undefined) {
				throw new Error('totalBefore is missing in the response');
			}

			const items = data.items as Array<TreeItemType>;

			// We have loaded previous items so we add them to the top of the array
			const reversedItems = [...items].reverse();
			this.#children.prepend(reversedItems);

			this.setHasChildren(data.total > 0);

			this.targetPagination.prependCurrentItems(reversedItems);
			this.targetPagination.setTotalItems(data.total);
			this.targetPagination.setTotalItemsBeforeStartTarget(data.totalBefore);
		}

		this.#isLoading.setValue(false);
		this.#isLoadingPrevChildren.setValue(false);
	}

	#loadNextItemsRetries = 0;
	async #loadNextItemsFromTarget() {
		if (this.#loadNextItemsRetries > this.#requestMaxRetries) {
			// If we have exceeded the maximum number of retries, we need to reset the base target and load from the top
			this.#loadNextItemsRetries = 0;
			this.#resetChildren('error');
			return;
		}

		const repository = this.#treeContext?.getRepository();
		if (!repository) throw new Error('Could not request children, repository is missing');

		this.#isLoading.setValue(true);
		this.#isLoadingNextChildren.setValue(true);

		const parent = this.getStartNode() || this.getTreeItem();
		const foldersOnly = this.getFoldersOnly();
		const additionalArgs = this.getAdditionalRequestArgs();
		const endTarget = this.targetPagination.getEndTarget();

		const targetPaging: UmbTargetPaginationRequestModel | undefined = {
			target: endTarget,
			takeBefore: 0,
			takeAfter: this.targetPagination.getTakeSize(),
		};

		const offsetPaging: UmbOffsetPaginationRequestModel = {
			skip: this.offsetPagination.getSkip(),
			take: this.offsetPagination.getPageSize(),
		};

		const { data, error } = parent?.unique
			? await repository.requestTreeItemsOf({
					parent: {
						unique: parent.unique,
						entityType: parent.entityType,
					},
					skip: offsetPaging.skip, // including this for backward compatibility
					take: offsetPaging.take, // including this for backward compatibility
					paging: targetPaging,
					foldersOnly,
					...additionalArgs,
				})
			: await repository.requestTreeRootItems({
					skip: offsetPaging.skip, // including this for backward compatibility
					take: offsetPaging.take, // including this for backward compatibility
					paging: targetPaging,
					foldersOnly,
					...additionalArgs,
				});

		if (error && error.message.includes('not found')) {
			this.#loadNextItemsRetries++;
			const newEndTarget = this.targetPagination.getNewEndTarget();
			this.targetPagination.removeFromCurrentItems(endTarget);

			if (newEndTarget) {
				this.#loadNextItemsFromTarget();
			} else {
				/*
					If we can't find a new end target we reload the children from the top.
					We cancel the base target and load using skip/take pagination instead.
				*/
				this.#resetChildren(this.#children.getValue().length === 0 ? 'empty' : 'fallback');
			}
		}

		if (data) {
			const items = data.items as Array<TreeItemType>;
			this.#children.append(items);
			this.setHasChildren(data.total > 0);

			this.offsetPagination.setTotalItems(data.total);

			this.targetPagination.appendCurrentItems(data.items);
			this.targetPagination.setTotalItems(data.total);
			this.targetPagination.setTotalItemsAfterEndTarget(data.totalAfter);

			this.#loadNextItemsRetries = 0;
		}

		this.#isLoading.setValue(false);
		this.#isLoadingNextChildren.setValue(false);
	}

	/**
	 * Checks if a specific child is loaded
	 * @param {(UmbEntityModel | undefined)} entity
	 * @returns {boolean} - True if items has been loaded
	 * @memberof UmbRepositoryTreeItemChildrenManager
	 */
	public isChildLoaded(entity: UmbEntityModel | undefined): boolean {
		return this.#children
			.getValue()
			.some((child) => child.entityType === entity?.entityType && entity.unique === child.unique);
	}

	/**
	 * Checks if any children have been loaded
	 * @returns {boolean} - True if any items has been loaded
	 * @memberof UmbRepositoryTreeItemChildrenManager
	 */
	public hasLoadedChildren(): boolean {
		return this.#children.getValue().length > 0;
	}

	/**
	 * Clears the internal state
	 * @memberof UmbRepositoryTreeItemChildrenManager
	 */
	public clear(): void {
		this.#children.setValue([]);
		this.offsetPagination.clear();
		this.targetPagination.clear();
	}

	/**
	 * Loads children using offset pagination only.
	 * This is a "safe" fallback that does NOT:
	 * - Use target pagination
	 * - Retry with new targets
	 * - Call #resetChildren (preventing recursion)
	 * - Throw errors (fails gracefully)
	 */
	async #loadChildrenWithOffsetPagination(): Promise<void> {
		const repository = this.#treeContext?.getRepository();
		if (!repository) {
			// Terminal fallback - fail silently rather than throwing
			return;
		}

		this.#isLoading.setValue(true);

		const parent = this.getStartNode() || this.getTreeItem();
		const foldersOnly = this.getFoldersOnly();
		const additionalArgs = this.getAdditionalRequestArgs();

		const offsetPaging: UmbOffsetPaginationRequestModel = {
			skip: 0, // Always from the start
			take: this.offsetPagination.getPageSize(),
		};

		const { data } = parent?.unique
			? await repository.requestTreeItemsOf({
					parent: { unique: parent.unique, entityType: parent.entityType },
					skip: offsetPaging.skip,
					take: offsetPaging.take,
					paging: offsetPaging,
					foldersOnly,
					...additionalArgs,
				})
			: await repository.requestTreeRootItems({
					skip: offsetPaging.skip,
					take: offsetPaging.take,
					paging: offsetPaging,
					foldersOnly,
					...additionalArgs,
				});

		if (data) {
			const items = data.items as Array<TreeItemType>;
			this.#children.setValue(items);
			this.setHasChildren(data.total > 0);
			this.offsetPagination.setTotalItems(data.total);
		}
		// Note: On error, we simply don't update state - UI shows stale data
		// This is the terminal fallback, no further recovery

		this.#isLoading.setValue(false);
	}

	async #resetChildren(reason: ResetReason = 'error'): Promise<void> {
		// Clear pagination state
		this.targetPagination.clear();
		this.offsetPagination.clear();

		// Reset retry counters to prevent any lingering retry state
		this.#loadChildrenRetries = 0;
		this.#loadPrevItemsRetries = 0;
		this.#loadNextItemsRetries = 0;

		// Load using offset pagination only - this is our terminal fallback
		await this.#loadChildrenWithOffsetPagination();

		// Only show notification for actual errors
		if (reason === 'error') {
			const notificationManager = await this.getContext(UMB_NOTIFICATION_CONTEXT);
			notificationManager?.peek('danger', {
				data: { message: 'Menu loading failed. Showing the first items again.' },
			});
		}
	}

	#onPageChange = () => this.loadNextChildren();

	#listenForActionEvents() {
		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (instance) => {
			this.#removeEventListeners();
			this.#actionEventContext = instance;

			this.#actionEventContext?.addEventListener(
				UmbRequestReloadTreeItemChildrenEvent.TYPE,
				this.#onReloadChildrenRequest as EventListener,
			);

			this.#actionEventContext?.addEventListener(
				UmbRequestReloadChildrenOfEntityEvent.TYPE,
				this.#onReloadChildrenRequest as EventListener,
			);

			this.#actionEventContext?.addEventListener(
				UmbRequestReloadStructureForEntityEvent.TYPE,
				this.#onReloadStructureForEntityRequest as unknown as EventListener,
			);
		});
	}

	#onReloadChildrenRequest = (event: UmbEntityActionEvent) => {
		const entityType = this.getTreeItem()?.entityType;
		const unique = this.getTreeItem()?.unique;

		if (event.getEntityType() !== entityType) return;
		if (event.getUnique() !== unique) return;

		this.reloadChildren();
	};

	#onReloadStructureForEntityRequest = async (event: UmbRequestReloadStructureForEntityEvent) => {
		const entityType = this.getTreeItem()?.entityType;
		const unique = this.getTreeItem()?.unique;

		if (event.getEntityType() !== entityType) return;
		if (event.getUnique() !== unique) return;

		if (this.#parentTreeItemContext) {
			this.#parentTreeItemContext.reloadChildren();
		} else if (this.#treeContext) {
			this.#treeContext.reloadTree();
		}
	};

	#removeEventListeners = () => {
		this.#actionEventContext?.removeEventListener(
			UmbRequestReloadTreeItemChildrenEvent.TYPE,
			this.#onReloadChildrenRequest as EventListener,
		);

		this.#actionEventContext?.removeEventListener(
			UmbRequestReloadChildrenOfEntityEvent.TYPE,
			this.#onReloadChildrenRequest as EventListener,
		);

		this.#actionEventContext?.removeEventListener(
			UmbRequestReloadStructureForEntityEvent.TYPE,
			this.#onReloadStructureForEntityRequest as unknown as EventListener,
		);
	};

	override destroy(): void {
		this.#removeEventListeners();
		super.destroy();
	}
}
