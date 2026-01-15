import type { ManifestTreeItem } from '../../extensions/types.js';
import type { UmbTreeItemContext } from '../tree-item-context.interface.js';
import type { UmbTreeItemModel, UmbTreeRootModel } from '../../types.js';
import { UmbTreeItemChildrenManager } from '../tree-item-children.manager.js';
import { UmbTreeItemEntityActionManager } from '../tree-item-entity-action.manager.js';
import { UmbTreeItemTargetExpansionManager } from '../tree-item-expansion.manager.js';
import { UMB_TREE_CONTEXT } from '../../tree.context.token.js';
import { UMB_TREE_ITEM_CONTEXT } from '../tree-item.context.token.js';
import { ensureSlash } from '@umbraco-cms/backoffice/router';
import { map } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbBooleanState, UmbObjectState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbDeprecation, debounce } from '@umbraco-cms/backoffice/utils';
import { UmbParentEntityContext } from '@umbraco-cms/backoffice/entity';
import { UMB_SECTION_CONTEXT } from '@umbraco-cms/backoffice/section';
import { UMB_WORKSPACE_EDIT_PATH_PATTERN } from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityModel, UmbEntityUnique } from '@umbraco-cms/backoffice/entity';

export abstract class UmbTreeItemContextBase<
		TreeItemType extends UmbTreeItemModel,
		TreeRootType extends UmbTreeRootModel,
		ManifestType extends ManifestTreeItem = ManifestTreeItem,
	>
	extends UmbContextBase
	implements UmbTreeItemContext<TreeItemType>
{
	#gotTreeContext!: Promise<unknown>;
	public unique?: UmbEntityUnique;
	public entityType?: string;

	#manifest?: ManifestType;

	protected readonly _treeItem = new UmbObjectState<TreeItemType | undefined>(undefined);
	readonly treeItem = this._treeItem.asObservable();

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

	protected readonly _treeItemChildrenManager = new UmbTreeItemChildrenManager<TreeItemType, TreeRootType>(this);
	public readonly childItems = this._treeItemChildrenManager.children;
	public readonly hasChildren = this._treeItemChildrenManager.hasChildren;
	public readonly foldersOnly = this._treeItemChildrenManager.foldersOnly;
	public readonly pagination = this._treeItemChildrenManager.offsetPagination;
	public readonly targetPagination = this._treeItemChildrenManager.targetPagination;
	public readonly isLoading = this._treeItemChildrenManager.isLoading;
	public readonly isLoadingPrevChildren = this._treeItemChildrenManager.isLoadingPrevChildren;
	public readonly isLoadingNextChildren = this._treeItemChildrenManager.isLoadingNextChildren;

	#treeItemExpansionManager = new UmbTreeItemTargetExpansionManager<TreeItemType, TreeRootType>(this, {
		childrenManager: this._treeItemChildrenManager,
		targetPaginationManager: this.targetPagination,
	});
	isOpen = this.#treeItemExpansionManager.isExpanded;

	#treeItemEntityActionManager = new UmbTreeItemEntityActionManager(this);
	public readonly hasActions = this.#treeItemEntityActionManager.hasActions;

	public treeContext?: typeof UMB_TREE_CONTEXT.TYPE;

	#sectionContext?: typeof UMB_SECTION_CONTEXT.TYPE;

	#parentContext = new UmbParentEntityContext(this);

	#hasActiveDescendant = new UmbBooleanState(undefined);
	public readonly hasActiveDescendant = this.#hasActiveDescendant.asObservable();

	#isMenu = false;
	setIsMenu(isMenu: boolean) {
		this.#isMenu = isMenu;
	}
	getIsMenu() {
		return this.#isMenu;
	}

	constructor(host: UmbControllerHost) {
		super(host, UMB_TREE_ITEM_CONTEXT);
		// TODO: Get take size from Tree context
		this._treeItemChildrenManager.setTakeSize(50);
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
	 * Returns the current path value
	 * @returns {string}
	 * @memberof UmbTreeItemContextBase
	 */
	public getPath() {
		return this.#path.getValue();
	}

	/**
	 * Returns the ascending items of this tree item
	 * @returns {Array<UmbEntityModel>}
	 * @memberof UmbTreeItemContextBase
	 */
	public getAscending(): Array<UmbEntityModel> | undefined {
		// This should be supported for all trees.
		return (this._treeItem.getValue() as any)?.ancestors;
	}

	/**
	 * Returns the manifest.
	 * @returns {ManifestCollection}
	 * @memberof UmbTreeItemContextBase
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

		this._treeItemChildrenManager.setTreeItem(treeItem);
		this.#treeItemExpansionManager.setTreeItem(treeItem);
		this.#treeItemEntityActionManager.setTreeItem(treeItem);

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
	public loadChildren = (): Promise<void> => this._treeItemChildrenManager.loadChildren();

	public reloadChildren = (): Promise<void> => this._treeItemChildrenManager.reloadChildren();

	/**
	 * Load more children of the tree item
	 * @deprecated Use `loadNextItems` instead. Will be removed in v18.0.0.
	 * @memberof UmbTreeItemContextBase
	 * @returns {Promise<void>}
	 */
	public loadMore = (): Promise<void> => this._treeItemChildrenManager.loadNextChildren();

	/**
	 * Load previous items of the tree item
	 * @memberof UmbTreeItemContextBase
	 * @returns {Promise<void>}
	 */
	public loadPrevItems = (): Promise<void> => this._treeItemChildrenManager.loadPrevChildren();

	/**
	 * Load next items of the tree item
	 * @memberof UmbTreeItemContextBase
	 * @returns {Promise<void>}
	 */
	public loadNextItems = (): Promise<void> => this._treeItemChildrenManager.loadNextChildren();

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
		// TODO: Stop consuming the section context, instead lets get the needed data from the tree context. [NL]
		this.consumeContext(UMB_SECTION_CONTEXT, (instance) => {
			this.#sectionContext = instance;
			this.#observeSectionPath();
		});

		this.#gotTreeContext = this.consumeContext(UMB_TREE_CONTEXT, (treeContext) => {
			this.treeContext = treeContext;
			this.#observeIsSelectable();
			this.#observeIsSelected();
			this.#observeFoldersOnly();
			this.#observeActive();
		}).asPromise();
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
				this._treeItemChildrenManager.setFoldersOnly(foldersOnly ?? false);
			},
			'observeFoldersOnly',
		);
	}

	#observeActive() {
		if (this.unique === undefined || this.entityType === undefined) return;

		const entity = { entityType: this.entityType, unique: this.unique };
		this.observe(
			this.treeContext?.activeManager.hasActiveDescendants(entity),
			(hasActiveDescendant) => {
				if (this.#hasActiveDescendant.getValue() === undefined && hasActiveDescendant === false) {
					return;
				}

				this.#hasActiveDescendant.setValue(hasActiveDescendant);
			},
			'observeActiveDescendant',
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

	#checkIsActive = async () => {
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

		if (this.#isActive.getValue() === isActive) return;
		if (!this.entityType || this.unique === undefined) {
			throw new Error('Could not check active state, entity type or unique is missing');
		}

		const ascending = this.getAscending();
		// Only if this type of item has ancestors...
		if (ascending) {
			const path = [...ascending, { entityType: this.entityType, unique: this.unique }];

			await this.#gotTreeContext;

			if (isActive) {
				this.treeContext?.activeManager.setActive(path);
			} else {
				// If this is the current, then remove it:
				// This is a hack, where we are assuming that another active item would have made its entrance and replaced the 'active' within 2 second. [NL]
				// The problem is that it may take some time before an item appears in the tree and communicates that its active.
				// And in the meantime the removal of this would have resulted in the parent closing. And since we don't use Active state to open the tree, then we have a problem.
				debounce(() => this.treeContext?.activeManager.removeActiveIfMatch(path), 1000);
			}
		}
		this.#isActive.setValue(isActive);
	};

	#debouncedCheckIsActive = debounce(this.#checkIsActive, 100);

	// TODO: use router context
	constructPath(pathname: string, entityType: string, unique: string | null) {
		return UMB_WORKSPACE_EDIT_PATH_PATTERN.generateAbsolute({
			sectionName: pathname,
			entityType,
			unique: unique ?? 'null',
		});
	}

	override destroy(): void {
		window.removeEventListener('navigationend', this.#debouncedCheckIsActive);
		super.destroy();
	}
}
