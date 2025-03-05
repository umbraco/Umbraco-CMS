import type { UmbTreeItemContext } from '../tree-item-context.interface.js';
import { UMB_TREE_CONTEXT, type UmbDefaultTreeContext } from '../../default/index.js';
import type { UmbTreeExpansionModel, UmbTreeItemModel, UmbTreeRootModel } from '../../types.js';
import { UmbRequestReloadTreeItemChildrenEvent } from '../../entity-actions/reload-tree-item-children/index.js';
import type { ManifestTreeItem } from '../../extensions/types.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { map } from '@umbraco-cms/backoffice/external/rxjs';
import {
	appendToFrozenArray,
	UmbArrayState,
	UmbBooleanState,
	UmbObjectState,
	UmbStringState,
} from '@umbraco-cms/backoffice/observable-api';
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
import { UmbPaginationManager, debounce } from '@umbraco-cms/backoffice/utils';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UmbEntityModel, UmbEntityUnique } from '@umbraco-cms/backoffice/entity';

export abstract class UmbTreeItemContextBase<
		TreeItemType extends UmbTreeItemModel,
		TreeRootType extends UmbTreeRootModel,
		ManifestType extends ManifestTreeItem = ManifestTreeItem,
	>
	extends UmbContextBase<UmbTreeItemContext<TreeItemType>>
	implements UmbTreeItemContext<TreeItemType>
{
	public unique?: UmbEntityUnique;
	public entityType?: string;
	public readonly pagination = new UmbPaginationManager();

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

	#expansion = new UmbObjectState<UmbTreeExpansionModel | undefined>(undefined);
	expansion = this.#expansion.asObservable();

	#foldersOnly = new UmbBooleanState(false);
	readonly foldersOnly = this.#foldersOnly.asObservable();

	public treeContext?: UmbDefaultTreeContext<TreeItemType, TreeRootType>;
	public parentTreeItemContext?: UmbTreeItemContext<TreeItemType>;

	#sectionContext?: typeof UMB_SECTION_CONTEXT.TYPE;
	#sectionSidebarContext?: typeof UMB_SECTION_SIDEBAR_CONTEXT.TYPE;
	#actionEventContext?: typeof UMB_ACTION_EVENT_CONTEXT.TYPE;

	#hasChildrenContext = new UmbHasChildrenEntityContext(this);

	// TODO: get this from the tree context
	#paging = {
		skip: 0,
		take: 50,
	};

	constructor(host: UmbControllerHost) {
		super(host, UMB_TREE_ITEM_CONTEXT);
		this.pagination.setPageSize(this.#paging.take);
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

	// TODO: Be aware that this method, could be removed and we can use the getter method instead [NL]
	/**
	 * Returns the manifest.
	 * @returns {ManifestCollection}
	 * @memberof UmbCollectionContext
	 */
	public getManifest() {
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
	 */
	public loadChildren = () => this.#loadChildren();

	/**
	 * Load more children of the tree item
	 * @memberof UmbTreeItemContextBase
	 */
	public loadMore = () => this.#loadChildren(true);

	async #loadChildren(loadMore = false) {
		if (this.unique === undefined) throw new Error('Could not request children, unique key is missing');
		if (this.entityType === undefined) throw new Error('Could not request children, entity type is missing');

		// TODO: wait for tree context to be ready
		const repository = this.treeContext?.getRepository();
		if (!repository) throw new Error('Could not request children, repository is missing');

		this.#isLoading.setValue(true);

		const skip = loadMore ? this.#paging.skip : 0;
		const take = loadMore ? this.#paging.take : this.pagination.getCurrentPageNumber() * this.#paging.take;
		const foldersOnly = this.#foldersOnly.getValue();
		const additionalArgs = this.treeContext?.getAdditionalRequestArgs();

		const { data } = await repository.requestTreeItemsOf({
			parent: {
				unique: this.unique,
				entityType: this.entityType,
			},
			foldersOnly,
			skip,
			take,
			...additionalArgs,
		});

		if (data) {
			if (loadMore) {
				const currentItems = this.#childItems.getValue();
				this.#childItems.setValue([...currentItems, ...data.items]);
			} else {
				this.#childItems.setValue(data.items);
			}

			const hasChildren = data.total > 0;
			this.#hasChildren.setValue(hasChildren);
			this.#hasChildrenContext.setHasChildren(hasChildren);

			this.pagination.setTotalItems(data.total);
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

	/**
	 * Opens a child tree item
	 * @param {UmbEntityModel} entity The entity to open
	 * @param {string} entity.entityType The entity type
	 * @param {string} entity.unique The unique key
	 * @memberof UmbTreeItemContextBase
	 * @returns {void}
	 */
	public openChild(entity: UmbEntityModel): void {
		const currentValue = this.#expansion.getValue() ?? [];
		const newValue = appendToFrozenArray(currentValue, { ...entity, expand: [] }, (x) => x?.unique);
		this.#expansion.setValue(newValue);
	}

	/**
	 * Closes a child tree item
	 * @param {UmbEntityModel} entity The entity to close
	 * @param {string} entity.entityType The entity type
	 * @param {string} entity.unique The unique key
	 * @memberof UmbTreeItemContextBase
	 * @returns {void}
	 */
	public closeChild(entity: UmbEntityModel): void {
		const currentValue = this.#expansion.getValue() ?? [];
		const newValue = currentValue.filter((x) => x.entityType !== entity.entityType && x.unique !== entity.unique);
		this.#expansion.setValue(newValue);
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

		// It is the parent that keeps track of the open children. We tell the parent to open this child
		const parentContext = this.parentTreeItemContext ?? this.treeContext;
		parentContext?.openChild({ entityType, unique });
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

		const parentContext = this.parentTreeItemContext ?? this.treeContext;
		parentContext?.closeChild({ entityType, unique });
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
			this.#observeExpansion(this.treeContext?.expansion);
		});

		this.consumeContext(UMB_TREE_ITEM_CONTEXT, (instance) => {
			this.parentTreeItemContext = instance;
			this.#observeExpansion(this.parentTreeItemContext.expansion);
		}).skipHost();

		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (instance) => {
			this.#removeEventListeners();
			this.#actionEventContext = instance;

			this.#actionEventContext.addEventListener(
				UmbRequestReloadTreeItemChildrenEvent.TYPE,
				this.#onReloadRequest as EventListener,
			);

			this.#actionEventContext.addEventListener(
				UmbRequestReloadChildrenOfEntityEvent.TYPE,
				this.#onReloadRequest as EventListener,
			);

			this.#actionEventContext.addEventListener(
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
		if (!this.treeContext || this.unique === undefined) return;

		this.observe(
			this.treeContext.foldersOnly,
			(foldersOnly) => {
				this.#foldersOnly.setValue(foldersOnly);
			},
			'observeFoldersOnly',
		);
	}

	#observeSectionPath() {
		if (!this.#sectionContext) return;

		this.observe(
			this.#sectionContext.pathname,
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

	#observeExpansion(expansion: Observable<UmbTreeExpansionModel | undefined> | undefined) {
		if (!expansion) return;

		this.observe(
			expansion,
			(expansion) => {
				if (this.unique === undefined) return;

				// Check if this item is in the expansion
				const isSelfExpanded = expansion?.find(
					(entry) => entry.entityType === this.entityType && entry.unique === this.unique,
				);

				// If this item has children, load them
				if (isSelfExpanded && this.#hasChildren.getValue()) {
					this.#expansion.setValue(isSelfExpanded.expand);
					this.loadChildren();
					this.#checkIsOpen(expansion);
				}
			},
			'observeLocation',
		);
	}

	#checkIsOpen(expansion: UmbTreeExpansionModel | undefined) {
		const isSelf = expansion?.find((entry) => entry.entityType === this.entityType && entry.unique === this.unique);
		const isOpen = isSelf ? true : false;
		this.#isOpen.setValue(isOpen);
	}

	#onReloadRequest = (event: UmbEntityActionEvent) => {
		if (event.getUnique() !== this.unique) return;
		if (event.getEntityType() !== this.entityType) return;
		this.loadChildren();
	};

	#onReloadStructureRequest = async (event: UmbRequestReloadStructureForEntityEvent) => {
		if (!this.unique) return;
		if (event.getUnique() !== this.unique) return;
		if (event.getEntityType() !== this.entityType) return;

		if (this.parentTreeItemContext) {
			this.parentTreeItemContext.loadChildren();
		} else {
			this.treeContext?.loadTree();
		}
	};

	#onPageChange = (event: UmbChangeEvent) => {
		const target = event.target as UmbPaginationManager;
		this.#paging.skip = target.getSkip();
		this.loadMore();
	};

	#debouncedCheckIsActive = debounce(() => this.#checkIsActive(), 100);

	#checkIsActive() {
		// don't set the active state if the item is selectable
		const isSelectable = this.#isSelectable.getValue();

		if (isSelectable) {
			this.#isActive.setValue(false);
			return;
		}

		const path = this.#path.getValue();
		const location = window.location.pathname;
		const isActive = location.includes(path);
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
