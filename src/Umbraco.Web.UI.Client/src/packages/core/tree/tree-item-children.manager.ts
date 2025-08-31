import type { UmbTreeRootItemsRequestArgs } from './data/index.js';
import type { UmbTreeItemModel, UmbTreeRootModel } from './types.js';
import { UmbRequestReloadTreeItemChildrenEvent } from './entity-actions/reload-tree-item-children/index.js';
import { UMB_TREE_ITEM_CONTEXT, type UmbTreeItemContext } from './tree-item/index.js';
import type { UmbDefaultTreeContext } from './default/index.js';
import { UMB_TREE_CONTEXT } from './default/index.js';
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
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';

export class UmbTreeItemChildrenManager<
	TreeItemType extends UmbTreeItemModel,
	RequestArgsType extends UmbTreeRootItemsRequestArgs = UmbTreeRootItemsRequestArgs,
> extends UmbControllerBase {
	public readonly offsetPagination = new UmbPaginationManager();
	public readonly targetPagination = new UmbTargetPaginationManager(this);

	#children = new UmbArrayState<TreeItemType>([], (x) => x.unique);
	public readonly children = this.#children.asObservable();

	#parent = new UmbObjectState<UmbEntityModel | undefined>(undefined);
	parent = this.#parent.asObservable();

	#foldersOnly = new UmbBooleanState(false);
	foldersOnly = this.#foldersOnly.asObservable();

	#additionalRequestArgs = new UmbObjectState<Partial<RequestArgsType> | object>({});
	public readonly additionalRequestArgs = this.#additionalRequestArgs.asObservable();

	#isLoading = new UmbBooleanState(false);
	readonly isLoading = this.#isLoading.asObservable();

	#takeSize: number = 5;
	#actionEventContext?: typeof UMB_ACTION_EVENT_CONTEXT.TYPE;

	#treeContext?: UmbDefaultTreeContext<TreeItemType, UmbTreeRootModel, RequestArgsType>;
	#parentTreeItemContext?: UmbTreeItemContext<TreeItemType>;

	constructor(host: UmbControllerHost) {
		super(host);
		// listen for page changes on the pagination manager
		this.offsetPagination.addEventListener(UmbChangeEvent.TYPE, this.#onPageChange);

		this.#listenForActionEvents();

		this.consumeContext(UMB_TREE_CONTEXT, (treeContext) => {
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
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
	 * @param {(UmbEntityModel | undefined)} parent - The parent entity model.
	 * @memberof UmbTreeItemChildrenManager
	 */
	public setParent(parent: UmbEntityModel | undefined) {
		this.#parent.setValue(parent);
	}

	/**
	 * Gets the parent for which to load children.
	 * @returns {UmbEntityModel | undefined} - The parent for the children
	 * @memberof UmbTreeItemChildrenManager
	 */
	public getParent(): UmbEntityModel | undefined {
		return this.#parent.getValue();
	}

	/**
	 * Sets additional request arguments that will be passed with the request.
	 * @param {Partial<RequestArgsType>} args
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
	 * Loads the children for the current parent.
	 * @returns {Promise<void>}
	 * @memberof UmbTreeItemChildrenManager
	 */
	public async loadChildren(): Promise<void> {
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

	async #loadChildren(reload = false) {
		const repository = this.#treeContext?.getRepository();
		if (!repository) throw new Error('Could not request children, repository is missing');

		this.#isLoading.setValue(true);

		const parent = this.getParent();
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
						takeBefore: reload ? this.targetPagination.getNumberOfCurrentItemsBeforeBaseTarget() : 5,
						takeAfter: reload
							? this.targetPagination.getNumberOfCurrentItemsAfterBaseTarget()
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

		const { data } = parent?.unique
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

		if (data) {
			this.#children.setValue(data.items);

			this.offsetPagination.setTotalItems(data.total);

			this.targetPagination.setCurrentItems(data.items);
			this.targetPagination.setTotalItems(data.total);
			this.targetPagination.setTotalItemsBeforeStartTarget(data.totalBefore);
			this.targetPagination.setTotalItemsAfterEndTarget(data.totalAfter);
		}

		this.#isLoading.setValue(false);
	}

	async #loadPrevItemsFromTarget() {
		const repository = this.#treeContext?.getRepository();
		if (!repository) throw new Error('Could not request children, repository is missing');

		this.#isLoading.setValue(true);

		const parent = this.getParent();
		const foldersOnly = this.getFoldersOnly();
		const additionalArgs = this.getAdditionalRequestArgs();

		const targetPaging: UmbTargetPaginationRequestModel | undefined = {
			target: this.targetPagination.getStartTarget(),
			takeBefore: this.targetPagination.getTakeSize(),
			takeAfter: 0,
		};

		const { data } = parent?.unique
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

		if (data) {
			// We have loaded previous items so we add them to the top of the array
			const reversedItems = [...data.items].reverse();
			this.#children.prepend(reversedItems);
			this.targetPagination.prependCurrentItems(reversedItems);

			if (data.totalBefore === undefined) {
				throw new Error('totalBefore is missing in the response');
			}

			this.targetPagination.setTotalItems(data.total);
			this.targetPagination.setTotalItemsBeforeStartTarget(data.totalBefore);
		}

		this.#isLoading.setValue(false);
	}

	async #loadNextItemsFromTarget() {
		const repository = this.#treeContext?.getRepository();
		if (!repository) throw new Error('Could not request children, repository is missing');

		this.#isLoading.setValue(true);

		const parent = this.getParent();
		const foldersOnly = this.getFoldersOnly();
		const additionalArgs = this.getAdditionalRequestArgs();

		const targetPaging: UmbTargetPaginationRequestModel | undefined = {
			target: this.targetPagination.getEndTarget(),
			takeBefore: 0,
			takeAfter: this.targetPagination.getTakeSize(),
		};

		const offsetPaging: UmbOffsetPaginationRequestModel = {
			skip: this.offsetPagination.getSkip(),
			take: this.offsetPagination.getPageSize(),
		};

		const { data } = parent?.unique
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

		if (data) {
			this.#children.append(data.items);

			this.offsetPagination.setTotalItems(data.total);

			this.targetPagination.appendCurrentItems(data.items);
			this.targetPagination.setTotalItems(data.total);
			this.targetPagination.setTotalItemsAfterEndTarget(data.totalAfter);
		}

		this.#isLoading.setValue(false);
	}

	/**
	 * Checks if a specific child is loaded
	 * @param {(UmbEntityModel | undefined)} item
	 * @returns {boolean} - True if items has been loaded
	 * @memberof UmbRepositoryTreeItemChildrenManager
	 */
	public isChildLoaded(item: UmbEntityModel | undefined): boolean {
		return this.#children
			.getValue()
			.some((child) => child.entityType === item?.entityType && item.unique === child.unique);
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
		const entityType = this.getParent()?.entityType;
		const unique = this.getParent()?.unique;

		if (event.getEntityType() !== entityType) return;
		if (event.getUnique() !== unique) return;

		this.reloadChildren();
	};

	#onReloadStructureForEntityRequest = async (event: UmbRequestReloadStructureForEntityEvent) => {
		const entityType = this.getParent()?.entityType;
		const unique = this.getParent()?.unique;

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
