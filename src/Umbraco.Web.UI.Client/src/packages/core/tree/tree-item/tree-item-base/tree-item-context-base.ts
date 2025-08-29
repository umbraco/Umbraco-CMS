import type { UmbTreeItemContext } from '../tree-item-context.interface.js';
import { UMB_TREE_CONTEXT, type UmbDefaultTreeContext } from '../../default/index.js';
import type { UmbTreeItemModel, UmbTreeRootModel } from '../../types.js';
import { UmbRequestReloadTreeItemChildrenEvent } from '../../entity-actions/reload-tree-item-children/index.js';
import type { ManifestTreeItem } from '../../extensions/types.js';
import { map } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbArrayState, UmbBooleanState, UmbObjectState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UMB_SECTION_CONTEXT, UMB_SECTION_SIDEBAR_CONTEXT } from '@umbraco-cms/backoffice/section';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import {
	UmbHasChildrenEntityContext,
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import type { UmbEntityActionEvent } from '@umbraco-cms/backoffice/entity-action';
import {
	UmbDeprecation,
	UmbPaginationManager,
	UmbTargetPaginationManager,
	debounce,
	type UmbOffsetPaginationRequestModel,
	type UmbTargetPaginationRequestModel,
} from '@umbraco-cms/backoffice/utils';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbParentEntityContext, type UmbEntityModel, type UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import { ensureSlash } from '@umbraco-cms/backoffice/router';

export abstract class UmbTreeItemContextBase<
		TreeItemType extends UmbTreeItemModel,
		TreeRootType extends UmbTreeRootModel,
		ManifestType extends ManifestTreeItem = ManifestTreeItem,
	>
	extends UmbContextBase
	implements UmbTreeItemContext<TreeItemType>
{
	public unique?: UmbEntityUnique;
	public entityType?: string;

	public readonly pagination = new UmbPaginationManager();
	public readonly targetPagination = new UmbTargetPaginationManager(this);

	#manifest?: ManifestType;

	protected readonly _treeItem = new UmbObjectState<TreeItemType | undefined>(undefined);
	readonly treeItem = this._treeItem.asObservable();

	// eslint-disable-next-line @typescript-eslint/ban-ts-comment
	// @ts-ignore
	#childItems = new UmbArrayState<TreeItemType>([], (x) => x.unique);
	readonly childItems = this.#childItems.asObservable();

	#hasChildren = new UmbBooleanState(false);
	readonly hasChildren = this.#hasChildren.asObservable();

	#isLoading = new UmbBooleanState(false);
	readonly isLoading = this.#isLoading.asObservable();

	#isSelectable = new UmbBooleanState(false);
	readonly isSelectable = this.#isSelectable.asObservable();

	#isSelectableContext = new UmbBooleanState(false);
	readonly isSelectableContext = this.#isSelectableContext.asObservable();

	#isSelected = new UmbBooleanState(false);
	readonly isSelected = this.#isSelected.asObservable();

	#isActive = new UmbBooleanState(false);
	readonly isActive = this.#isActive.asObservable();

	#hasActions = new UmbBooleanState(false);
	readonly hasActions = this.#hasActions.asObservable();

	#path = new UmbStringState('');
	readonly path = this.#path.asObservable();

	#isOpen = new UmbBooleanState(false);
	isOpen = this.#isOpen.asObservable();

	#foldersOnly = new UmbBooleanState(false);
	readonly foldersOnly = this.#foldersOnly.asObservable();

	public treeContext?: UmbDefaultTreeContext<TreeItemType, TreeRootType>;
	public parentTreeItemContext?: UmbTreeItemContext<TreeItemType>;

	#sectionContext?: typeof UMB_SECTION_CONTEXT.TYPE;
	#sectionSidebarContext?: typeof UMB_SECTION_SIDEBAR_CONTEXT.TYPE;
	#actionEventContext?: typeof UMB_ACTION_EVENT_CONTEXT.TYPE;

	#hasChildrenContext = new UmbHasChildrenEntityContext(this);
	#parentContext = new UmbParentEntityContext(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_TREE_ITEM_CONTEXT);

		const take = 5;
		this.pagination.setPageSize(take);
		this.targetPagination.setTakeSize(take);

		this.#consumeContexts();

		// listen for page changes on the pagination manager
		this.pagination.addEventListener(UmbChangeEvent.TYPE, this.#onPageChange);

		window.addEventListener('navigationend', this.#debouncedCheckIsActive);
	}

	/**
	 * Sets the manifest
	 * @param {ManifestCollection} manifest
	 * @memberof UmbCollectionContext
	 */
	public set manifest(manifest: ManifestType | undefined) {
		if (this.#manifest === manifest) return;
		this.#manifest = manifest;
	}
	public get manifest() {
		return this.#manifest;
	}

	/**
	 * Returns the manifest.
	 * @returns {ManifestCollection}
	 * @memberof UmbCollectionContext
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

	public setTreeItem(treeItem: TreeItemType | undefined) {
		if (!treeItem) {
			this._treeItem.setValue(undefined);
			return;
		}

		// Only check for undefined. The tree root has null as unique
		if (treeItem.unique === undefined) throw new Error('Could not create tree item context, unique is missing');
		this.unique = treeItem.unique;

		if (!treeItem.entityType) throw new Error('Could not create tree item context, tree item type is missing');
		this.entityType = treeItem.entityType;

		const hasChildren = treeItem.hasChildren || false;
		this.#hasChildren.setValue(hasChildren);
		this.#hasChildrenContext.setHasChildren(hasChildren);

		const parentEntity: UmbEntityModel | undefined = treeItem.parent
			? {
					entityType: treeItem.parent.entityType,
					unique: treeItem.parent.unique,
				}
			: undefined;
		this.#parentContext.setParent(parentEntity);
		this._treeItem.setValue(treeItem);

		// Update observers:
		this.#observeActions();
		this.#observeIsSelectable();
		this.#observeIsSelected();
		this.#observeSectionPath();
	}

	/**
	 * Load children of the tree item
	 * @memberof UmbTreeItemContextBase
	 * @returns {Promise<void>}
	 */
	public loadChildren = (): Promise<void> => this.#loadChildren();

	public reloadChildren = (): Promise<void> => this.#loadChildren(true);

	/**
	 * Load more children of the tree item
	 * @deprecated Use `loadNextItems` instead. Will be removed in v18.0.0.
	 * @memberof UmbTreeItemContextBase
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

	async #loadChildren(reload = false) {
		if (this.unique === undefined) throw new Error('Could not request children, unique key is missing');
		if (this.entityType === undefined) throw new Error('Could not request children, entity type is missing');

		// TODO: wait for tree context to be ready
		const repository = this.treeContext?.getRepository();
		if (!repository) throw new Error('Could not request children, repository is missing');

		this.#isLoading.setValue(true);

		const foldersOnly = this.#foldersOnly.getValue();
		const additionalArgs = this.treeContext?.getAdditionalRequestArgs();
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
			skip: reload ? 0 : this.pagination.getSkip(),
			take: reload
				? this.pagination.getCurrentPageNumber() * this.pagination.getPageSize()
				: this.pagination.getPageSize(),
		};

		const { data } = await repository.requestTreeItemsOf({
			parent: {
				unique: this.unique,
				entityType: this.entityType,
			},
			skip: offsetPaging.skip, // including this for backward compatibility
			take: offsetPaging.take, // including this for backward compatibility
			paging: targetPaging || offsetPaging,
			foldersOnly,
			...additionalArgs,
		});

		if (data) {
			this.#childItems.setValue(data.items);
			this.targetPagination.setCurrentItems(data.items);

			const hasChildren = data.total > 0;
			this.#hasChildren.setValue(hasChildren);
			this.#hasChildrenContext.setHasChildren(hasChildren);

			this.pagination.setTotalItems(data.total);
			this.targetPagination.setTotalItems(data.total);
			this.targetPagination.setTotalItemsBeforeStartTarget(data.totalBefore);
			this.targetPagination.setTotalItemsAfterEndTarget(data.totalAfter);
		}

		this.#isLoading.setValue(false);
	}

	async #loadPrevItemsFromTarget() {
		if (this.unique === undefined) throw new Error('Could not request children, unique key is missing');
		if (this.entityType === undefined) throw new Error('Could not request children, entity type is missing');

		const repository = this.treeContext?.getRepository();
		if (!repository) throw new Error('Could not request children, repository is missing');

		this.#isLoading.setValue(true);

		const foldersOnly = this.#foldersOnly.getValue();
		const additionalArgs = this.treeContext?.getAdditionalRequestArgs();

		const targetPaging: UmbTargetPaginationRequestModel | undefined = {
			target: this.targetPagination.getStartTarget(),
			takeBefore: this.targetPagination.getTakeSize(),
			takeAfter: 0,
		};

		const { data } = await repository.requestTreeItemsOf({
			parent: {
				unique: this.unique,
				entityType: this.entityType,
			},
			foldersOnly,
			paging: targetPaging,
			...additionalArgs,
		});

		if (data) {
			// We have loaded previous items so we add them to the top of the array
			const reversedItems = [...data.items].reverse();
			this.#childItems.prepend(reversedItems);
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
		if (this.unique === undefined) throw new Error('Could not request next items, unique key is missing');
		if (this.entityType === undefined) throw new Error('Could not request next items, entity type is missing');

		const repository = this.treeContext?.getRepository();
		if (!repository) throw new Error('Could not request next items, repository is missing');

		this.#isLoading.setValue(true);

		const foldersOnly = this.#foldersOnly.getValue();
		const additionalArgs = this.treeContext?.getAdditionalRequestArgs();

		const targetPaging: UmbTargetPaginationRequestModel | undefined = {
			target: this.targetPagination.getEndTarget(),
			takeBefore: 0,
			takeAfter: this.targetPagination.getTakeSize(),
		};

		const { data } = await repository.requestTreeItemsOf({
			parent: {
				unique: this.unique,
				entityType: this.entityType,
			},
			take: this.pagination.getPageSize(), // including this for backward compatibility
			skip: this.pagination.getSkip(), // including this for backward compatibility
			foldersOnly,
			paging: targetPaging,
			...additionalArgs,
		});

		if (data) {
			this.#childItems.append(data.items);
			this.targetPagination.appendCurrentItems(data.items);

			this.targetPagination.setTotalItems(data.total);
			this.targetPagination.setTotalItemsAfterEndTarget(data.totalAfter);
		}

		this.#isLoading.setValue(false);
	}

	public toggleContextMenu() {
		if (!this.getTreeItem() || !this.entityType || this.unique === undefined) {
			throw new Error('Could not request children, tree item is not set');
		}

		this.#sectionSidebarContext?.toggleContextMenu(this.getHostElement(), {
			entityType: this.entityType,
			unique: this.unique,
			headline: this.getTreeItem()?.name || '',
		});
	}

	/**
	 * Selects the tree item
	 * @memberof UmbTreeItemContextBase
	 * @returns {void}
	 */
	public select() {
		if (this.unique === undefined) throw new Error('Could not select. Unique is missing');
		this.treeContext?.selection.select(this.unique);
	}

	/**
	 * Deselects the tree item
	 * @memberof UmbTreeItemContextBase
	 * @returns {void}
	 */
	public deselect() {
		if (this.unique === undefined) throw new Error('Could not deselect. Unique is missing');
		this.treeContext?.selection.deselect(this.unique);
	}

	public showChildren() {
		const entityType = this.entityType;
		const unique = this.unique;

		if (!entityType) {
			throw new Error('Could not show children, entity type is missing');
		}

		if (unique === undefined) {
			throw new Error('Could not show children, unique is missing');
		}

		// It is the tree that keeps track of the open children. We tell the tree to open this child
		this.treeContext?.expansion.expandItem({ entityType, unique });
	}

	public hideChildren() {
		const entityType = this.entityType;
		const unique = this.unique;

		if (!entityType) {
			throw new Error('Could not show children, entity type is missing');
		}

		if (unique === undefined) {
			throw new Error('Could not show children, unique is missing');
		}

		this.treeContext?.expansion.collapseItem({ entityType, unique });
	}

	async #consumeContexts() {
		this.consumeContext(UMB_SECTION_CONTEXT, (instance) => {
			this.#sectionContext = instance;
			this.#observeSectionPath();
		});

		this.consumeContext(UMB_SECTION_SIDEBAR_CONTEXT, (instance) => {
			this.#sectionSidebarContext = instance;
		});

		this.consumeContext(UMB_TREE_CONTEXT, (treeContext) => {
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			this.treeContext = treeContext;
			this.#observeIsSelectable();
			this.#observeIsSelected();
			this.#observeFoldersOnly();
			this.#observeExpansion();
		});

		this.consumeContext(UMB_TREE_ITEM_CONTEXT, (instance) => {
			this.parentTreeItemContext = instance;
		}).skipHost();

		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (instance) => {
			this.#removeEventListeners();
			this.#actionEventContext = instance;

			this.#actionEventContext?.addEventListener(
				UmbRequestReloadTreeItemChildrenEvent.TYPE,
				this.#onReloadRequest as EventListener,
			);

			this.#actionEventContext?.addEventListener(
				UmbRequestReloadChildrenOfEntityEvent.TYPE,
				this.#onReloadRequest as EventListener,
			);

			this.#actionEventContext?.addEventListener(
				UmbRequestReloadStructureForEntityEvent.TYPE,
				this.#onReloadStructureRequest as unknown as EventListener,
			);
		});
	}

	getTreeItem() {
		return this._treeItem.getValue();
	}

	#observeIsSelectable() {
		if (!this.treeContext) return;
		this.observe(
			this.treeContext.selection.selectable,
			(value) => {
				this.#isSelectableContext.setValue(value);

				// If the tree is selectable, check if this item is selectable
				if (value === true) {
					const isSelectable = this.treeContext?.selectableFilter?.(this.getTreeItem()!) ?? true;
					this.#isSelectable.setValue(isSelectable);
					this.#checkIsActive();
				}
			},
			'observeIsSelectable',
		);
	}

	#observeIsSelected() {
		if (!this.treeContext || !this.unique) return;

		this.observe(
			this.treeContext.selection.selection.pipe(map((selection) => selection.includes(this.unique!))),
			(isSelected) => {
				this.#isSelected.setValue(isSelected);
			},
			'observeIsSelected',
		);
	}

	#observeFoldersOnly() {
		if (this.unique === undefined) return;

		this.observe(
			this.treeContext?.foldersOnly,
			(foldersOnly) => {
				this.#foldersOnly.setValue(foldersOnly ?? false);
			},
			'observeFoldersOnly',
		);
	}

	#observeSectionPath() {
		this.observe(
			this.#sectionContext?.pathname,
			(pathname) => {
				if (!pathname || !this.entityType || this.unique === undefined) return;
				const path = this.constructPath(pathname, this.entityType, this.unique);
				this.#path.setValue(path);
				this.#checkIsActive();
			},
			'observeSectionPath',
		);
	}

	#observeActions() {
		this.observe(
			umbExtensionsRegistry
				.byType('entityAction')
				.pipe(map((actions) => actions.filter((action) => action.forEntityTypes.includes(this.entityType!)))),
			(actions) => {
				this.#hasActions.setValue(actions.length > 0);
			},
			'observeActions',
		);
	}

	#observeExpansion() {
		if (this.unique === undefined) return;
		if (!this.entityType) return;

		const entity: UmbEntityModel = {
			entityType: this.entityType,
			unique: this.unique,
		};

		this.observe(
			this.treeContext?.expansion.entry(entity),
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
				const targetIsLoaded = this.#childItems
					.getValue()
					.some((child) => child.entityType === newTarget?.entityType && newTarget.unique === child.unique);

				if (newTarget && targetIsLoaded) {
					return;
				}

				// If this item is expanded and has children, load them
				if (isExpanded && this.#hasChildren.getValue()) {
					this.targetPagination.setBaseTarget(entry.target);
					this.#loadChildren();
				}

				this.#isOpen.setValue(isExpanded);
			},
			'observeExpansion',
		);
	}

	#onReloadRequest = (event: UmbEntityActionEvent) => {
		if (event.getUnique() !== this.unique) return;
		if (event.getEntityType() !== this.entityType) return;
		this.reloadChildren();
	};

	#onReloadStructureRequest = async (event: UmbRequestReloadStructureForEntityEvent) => {
		if (!this.unique) return;
		if (event.getUnique() !== this.unique) return;
		if (event.getEntityType() !== this.entityType) return;

		if (this.parentTreeItemContext) {
			this.parentTreeItemContext.reloadChildren();
		} else {
			this.treeContext?.reloadTree();
		}
	};

	#onPageChange = () => this.#loadNextItemsFromTarget();

	#debouncedCheckIsActive = debounce(() => this.#checkIsActive(), 100);

	#checkIsActive() {
		// don't set the active state if the item is selectable
		const isSelectable = this.#isSelectable.getValue();

		if (isSelectable) {
			this.#isActive.setValue(false);
			return;
		}

		/* Check if the current location includes the path of this tree item.
		We ensure that the paths ends with a slash to avoid collisions with paths like /path-1 and /path-1-2 where /path-1 is in both.
		Instead we compare /path-1/ with /path-1-2/ which wont collide.*/
		const location = ensureSlash(window.location.pathname);
		const comparePath = ensureSlash(this.#path.getValue());
		const isActive = location.includes(comparePath);
		this.#isActive.setValue(isActive);
	}

	// TODO: use router context
	constructPath(pathname: string, entityType: string, unique: string | null) {
		// TODO: Encode uniques [NL]
		return `section/${pathname}/workspace/${entityType}/edit/${unique}`;
	}

	#removeEventListeners = () => {
		this.#actionEventContext?.removeEventListener(
			UmbRequestReloadTreeItemChildrenEvent.TYPE,
			this.#onReloadRequest as EventListener,
		);

		this.#actionEventContext?.removeEventListener(
			UmbRequestReloadChildrenOfEntityEvent.TYPE,
			this.#onReloadRequest as EventListener,
		);

		this.#actionEventContext?.removeEventListener(
			UmbRequestReloadStructureForEntityEvent.TYPE,
			this.#onReloadStructureRequest as unknown as EventListener,
		);
	};

	override destroy(): void {
		this.#removeEventListeners();
		window.removeEventListener('navigationend', this.#debouncedCheckIsActive);
		super.destroy();
	}
}

export const UMB_TREE_ITEM_CONTEXT = new UmbContextToken<UmbTreeItemContext<any>>('UmbTreeItemContext');
