import type { UmbTreeItemContext } from '../tree-item-context.interface.js';
import { UMB_TREE_CONTEXT, type UmbDefaultTreeContext } from '../../default/index.js';
import type { UmbTreeItemModel, UmbTreeRootModel } from '../../types.js';
import type { ManifestTreeItem } from '../../extensions/types.js';
import { UmbTreeItemChildrenManager } from '../../tree-item-children.manager.js';
import { UmbTreeItemEntityActionManager } from '../../tree-item-entity-action.managet.js';
import { map } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbBooleanState, UmbObjectState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UMB_SECTION_CONTEXT, UMB_SECTION_SIDEBAR_CONTEXT } from '@umbraco-cms/backoffice/section';
import { UmbHasChildrenEntityContext } from '@umbraco-cms/backoffice/entity-action';
import { UmbDeprecation, debounce } from '@umbraco-cms/backoffice/utils';
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

	#manifest?: ManifestType;

	protected readonly _treeItem = new UmbObjectState<TreeItemType | undefined>(undefined);
	readonly treeItem = this._treeItem.asObservable();

	#hasChildren = new UmbBooleanState(false);
	readonly hasChildren = this.#hasChildren.asObservable();

	#isSelectable = new UmbBooleanState(false);
	readonly isSelectable = this.#isSelectable.asObservable();

	#isSelectableContext = new UmbBooleanState(false);
	readonly isSelectableContext = this.#isSelectableContext.asObservable();

	#isSelected = new UmbBooleanState(false);
	readonly isSelected = this.#isSelected.asObservable();

	#isActive = new UmbBooleanState(false);
	readonly isActive = this.#isActive.asObservable();

	#path = new UmbStringState('');
	readonly path = this.#path.asObservable();

	#isOpen = new UmbBooleanState(false);
	isOpen = this.#isOpen.asObservable();

	#treeItemChildrenManager = new UmbTreeItemChildrenManager<TreeItemType>(this);
	public readonly childItems = this.#treeItemChildrenManager.children;
	public readonly foldersOnly = this.#treeItemChildrenManager.foldersOnly;
	public readonly pagination = this.#treeItemChildrenManager.offsetPagination;
	public readonly targetPagination = this.#treeItemChildrenManager.targetPagination;
	public readonly isLoading = this.#treeItemChildrenManager.isLoading;

	#treeItemEntityActionManager = new UmbTreeItemEntityActionManager(this);
	public readonly hasActions = this.#treeItemEntityActionManager.hasActions;

	public treeContext?: UmbDefaultTreeContext<TreeItemType, TreeRootType>;

	#sectionContext?: typeof UMB_SECTION_CONTEXT.TYPE;
	#sectionSidebarContext?: typeof UMB_SECTION_SIDEBAR_CONTEXT.TYPE;

	#hasChildrenContext = new UmbHasChildrenEntityContext(this);
	#parentContext = new UmbParentEntityContext(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_TREE_ITEM_CONTEXT);
		this.#treeItemChildrenManager.setTakeSize(5);
		this.#consumeContexts();
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

		this.#treeItemChildrenManager.setParent({ entityType: treeItem.entityType, unique: treeItem.unique });
		this.#treeItemEntityActionManager.setEntity({ entityType: treeItem.entityType, unique: treeItem.unique });

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
		this.#observeIsSelectable();
		this.#observeIsSelected();
		this.#observeSectionPath();
	}

	/**
	 * Load children of the tree item
	 * @memberof UmbTreeItemContextBase
	 * @returns {Promise<void>}
	 */
	public loadChildren = (): Promise<void> => this.#treeItemChildrenManager.loadChildren();

	public reloadChildren = (): Promise<void> => this.#treeItemChildrenManager.reloadChildren();

	/**
	 * Load more children of the tree item
	 * @deprecated Use `loadNextItems` instead. Will be removed in v18.0.0.
	 * @memberof UmbTreeItemContextBase
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
				this.#treeItemChildrenManager.setFoldersOnly(foldersOnly ?? false);
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
				const targetIsLoaded = this.#treeItemChildrenManager.isChildLoaded(newTarget);

				if (newTarget && targetIsLoaded) {
					return;
				}

				// If we already have children and the target didn't change then we don't have to load new children
				if (isExpanded && this.#treeItemChildrenManager.hasLoadedChildren()) {
					return;
				}

				// If this item is expanded and has children, load them
				if (isExpanded && this.#hasChildren.getValue()) {
					this.targetPagination.setBaseTarget(newTarget);
					this.#treeItemChildrenManager.loadChildren();
				}

				this.#isOpen.setValue(isExpanded);
			},
			'observeExpansion',
		);
	}

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

	override destroy(): void {
		window.removeEventListener('navigationend', this.#debouncedCheckIsActive);
		super.destroy();
	}
}

export const UMB_TREE_ITEM_CONTEXT = new UmbContextToken<UmbTreeItemContext<any>>('UmbTreeItemContext');
