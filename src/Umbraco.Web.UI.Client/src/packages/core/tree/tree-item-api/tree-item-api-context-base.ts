import type { UmbTreeItemModel } from '../types.js';
import { UMB_TREE_CONTEXT } from '../tree.context.token.js';
import { UMB_TREE_ITEM_BASE_CONTEXT } from '../tree-item/tree-item.context.token.js';
import type { UmbTreeItemApi } from './tree-item-api.interface.js';
import { combineLatest, distinctUntilChanged, map } from '@umbraco-cms/backoffice/external/rxjs';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbBooleanState, UmbObjectState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_WORKSPACE_EDIT_PATH_PATTERN } from '@umbraco-cms/backoffice/workspace';
import { debounce, UmbDeprecation } from '@umbraco-cms/backoffice/utils';
import { UmbEntityContext, UmbParentEntityContext } from '@umbraco-cms/backoffice/entity';
import { UMB_SECTION_CONTEXT } from '@umbraco-cms/backoffice/section';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityModel, UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import type { ManifestBase } from '@umbraco-cms/backoffice/extension-api';

/**
 * Abstract base for tree item apis. Handles item data, selection, active state,
 * path, and entity actions — without children, expansion, or pagination.
 *
 * Provides itself as `UMB_TREE_ITEM_BASE_CONTEXT` so entity action conditions
 * can discover a tree item regardless of which tree view is active.
 */
export abstract class UmbTreeItemApiContextBase<
	TreeItemType extends UmbTreeItemModel = UmbTreeItemModel,
	ManifestType extends ManifestBase = ManifestBase,
>
	extends UmbContextBase
	implements UmbTreeItemApi<TreeItemType, ManifestType>
{
	public unique: UmbEntityUnique | undefined;
	public entityType: string | undefined;

	#manifest?: ManifestType;
	public get manifest(): ManifestType | undefined {
		return this.#manifest;
	}
	public set manifest(value: ManifestType | undefined) {
		if (this.#manifest === value) return;
		this.#manifest = value;
	}

	protected _treeContext?: typeof UMB_TREE_CONTEXT.TYPE;

	/** Exposes the tree context consumer so subclasses can call `.asPromise()` on it. */
	protected readonly _treeContextConsumer;

	readonly #gotTreeContext: Promise<unknown>;

	protected readonly _treeItem = new UmbObjectState<TreeItemType | undefined>(undefined);
	readonly treeItem = this._treeItem.asObservable();

	protected readonly _isSelectable = new UmbBooleanState(false);
	readonly isSelectable = this._isSelectable.asObservable();

	#isSelectableContext = new UmbBooleanState(false);
	readonly isSelectableContext = this.#isSelectableContext.asObservable();

	protected readonly _isSelected = new UmbBooleanState(false);
	readonly isSelected = this._isSelected.asObservable();

	protected readonly _isActive = new UmbBooleanState(false);
	readonly isActive = this._isActive.asObservable();

	readonly hasChildren = this._treeItem.asObservablePart((item) => item?.hasChildren ?? false);

	#hasActiveDescendant = new UmbBooleanState(undefined);
	readonly hasActiveDescendant = this.#hasActiveDescendant.asObservable();

	#hideTreeItemActions = new UmbBooleanState(false);

	readonly noAccess = this._treeItem.asObservablePart((item) => item?.noAccess ?? false);

	protected readonly _drillable = new UmbBooleanState(false);
	/**
	 * Whether opening this item takes the user into it, as answered by the tree — it is a property of the tree's host,
	 * the same for every item in it.
	 *
	 * False until the tree says otherwise, so a host that cannot is never mistaken for one that can.
	 */
	readonly drillable = this._drillable.asObservable();

	/**
	 * @returns {Observable<boolean>} True if any entity action is registered for this entity type
	 * @deprecated Deprecated since v17. This only tells whether a manifest exists for the entity type, it does not
	 * evaluate the conditions of the actions. Render `<umb-entity-actions-bundle>`, which resolves the actions
	 * that are actually permitted for the item. Will be removed in v19.
	 */
	get hasActions(): Observable<boolean> {
		new UmbDeprecation({
			deprecated: 'UmbTreeItemApiContextBase.hasActions',
			removeInVersion: '19.0.0',
			solution:
				'Render <umb-entity-actions-bundle>, which resolves the entity actions that are permitted for the item.',
		}).warn();

		return combineLatest([
			this._treeItem.asObservablePart((item) => item?.entityType),
			umbExtensionsRegistry.byType('entityAction'),
			this.#hideTreeItemActions.asObservable(),
		]).pipe(
			map(
				([entityType, actions, hide]) =>
					!hide && !!entityType && actions.some((action) => action.forEntityTypes.includes(entityType)),
			),
			distinctUntilChanged(),
		);
	}

	protected readonly _selectOnly = new UmbBooleanState(false);
	readonly selectOnly = this._selectOnly.asObservable();

	#path = new UmbStringState('');
	readonly path = this.#path.asObservable();

	#sectionContext?: typeof UMB_SECTION_CONTEXT.TYPE;
	#entityContext = new UmbEntityContext(this);
	#parentContext = new UmbParentEntityContext(this);

	/**
	 * Public accessor for the tree context. Kept public for backward compatibility.
	 * @returns {typeof UMB_TREE_CONTEXT.TYPE | undefined} The tree context
	 */
	public get treeContext(): typeof UMB_TREE_CONTEXT.TYPE | undefined {
		return this._treeContext;
	}

	constructor(host: UmbControllerHost) {
		super(host, UMB_TREE_ITEM_BASE_CONTEXT);

		this._treeContextConsumer = this.consumeContext(UMB_TREE_CONTEXT, (context) => {
			this._treeContext = context;
			this._observeIsSelectable();
			this._observeIsSelected();
			this._observeSelectOnly();
			this._observeDrillable();
			if (context) this._onTreeContextChanged(context);
		});
		this.#gotTreeContext = this._treeContextConsumer.asPromise();

		this.consumeContext(UMB_SECTION_CONTEXT, (instance) => {
			this.#sectionContext = instance;
			this.#observeSectionPath();
		});
	}

	/**
	 * Returns whether opening this item takes the user into it.
	 * @returns {boolean} True when the tree's host enters opened items.
	 * @memberof UmbTreeItemApiContextBase
	 */
	getDrillable(): boolean {
		return this._drillable.getValue();
	}

	setTreeItem(item: TreeItemType | undefined): void {
		if (!item) {
			this._treeItem.setValue(undefined);
			this.#entityContext.setEntityType(undefined);
			this.#entityContext.setUnique(null);
			return;
		}

		// Only check for undefined. The tree root has null as unique.
		if (item.unique === undefined) throw new Error('Could not set tree item, unique is missing');
		if (!item.entityType) throw new Error('Could not set tree item, entity type is missing');

		if (item === this._treeItem.getValue()) return;

		this._treeItem.setValue(item);
		this.unique = item.unique;
		this.entityType = item.entityType;

		this.#entityContext.setEntityType(item.entityType);
		this.#entityContext.setUnique(item.unique);

		const parentEntity: UmbEntityModel | undefined = item.parent
			? { entityType: item.parent.entityType, unique: item.parent.unique }
			: undefined;
		this.#parentContext.setParent(parentEntity);

		this._observeIsSelected();
		this._observeIsSelectable();
		this.#observeSectionPath();
	}

	getTreeItem(): TreeItemType | undefined {
		return this._treeItem.getValue();
	}

	public getPath(): string {
		return this.#path.getValue();
	}

	public getAscending(): Array<UmbEntityModel> | undefined {
		return (this._treeItem.getValue() as any)?.ancestors;
	}

	protected _observeIsSelectable() {
		const ctx = this._treeContext;
		if (!ctx) return;
		this.observe(
			ctx.selection.selectable,
			(value) => {
				this.#isSelectableContext.setValue(value ?? false);
				const isSelectable = value ? (ctx.selectableFilter?.(this.getTreeItem()!) ?? true) : false;
				this._isSelectable.setValue(isSelectable);
				if (value === true) {
					this.#applyActiveState();
				}
			},
			'_observeIsSelectable',
		);
	}

	protected _observeIsSelected() {
		const ctx = this._treeContext;
		if (!ctx || this.unique === undefined) return;
		this.observe(
			ctx.selection.selection.pipe(map((selection) => selection.includes(this.unique!))),
			(isSelected) => {
				this._isSelected.setValue(isSelected);
			},
			'_observeIsSelected',
		);
	}

	protected _observeDrillable() {
		const ctx = this._treeContext;
		if (!ctx) return;
		this.observe(ctx.drillable, (value) => this._drillable.setValue(value ?? false), '_observeDrillable');
	}

	protected _observeSelectOnly() {
		const ctx = this._treeContext;
		if (!ctx) return;
		this.observe(ctx.selectOnly, (value) => this._selectOnly.setValue(value ?? false), '_observeSelectOnly');
	}

	/**
	 * Hook called when the tree context is received or changes. Subclasses can override to add additional observations.
	 * @param {typeof UMB_TREE_CONTEXT.TYPE} _context - The tree context
	 */
	protected _onTreeContextChanged(_context: typeof UMB_TREE_CONTEXT.TYPE): void {
		this.#observeActive();
		this.#observeIsCurrentLocation();
		if (_context.hideTreeItemActions) {
			this.observe(
				_context.hideTreeItemActions,
				(value) => this.#hideTreeItemActions.setValue(value ?? false),
				'_observeHideTreeItemActions',
			);
		}
	}

	#observeActive() {
		if (this.unique === undefined || this.entityType === undefined) return;

		const entity = { entityType: this.entityType, unique: this.unique };
		this.observe(
			this._treeContext?.activeManager.hasActiveDescendants(entity),
			(hasActiveDescendant) => {
				if (this.#hasActiveDescendant.getValue() === undefined && hasActiveDescendant === false) {
					return;
				}
				this.#hasActiveDescendant.setValue(hasActiveDescendant);
			},
			'observeActiveDescendant',
		);
	}

	#observeIsCurrentLocation() {
		this.observe(
			this._treeContext?.activeManager.isCurrentLocation(this.path),
			(isCurrentLocation) => {
				this.#isCurrentLocation = isCurrentLocation ?? false;
				this.#applyActiveState();
			},
			'observeIsCurrentLocation',
		);
	}

	#observeSectionPath() {
		this.observe(
			this.#sectionContext?.pathname,
			(pathname) => {
				if (!pathname || !this.entityType || this.unique === undefined) return;
				const path = this.constructPath(pathname, this.entityType, this.unique);
				this.#path.setValue(path);
			},
			'observeSectionPath',
		);
	}

	#isCurrentLocation = false;

	#applyActiveState = async () => {
		const isSelectable = this._isSelectable.getValue();

		if (isSelectable) {
			this._isActive.setValue(false);
			return;
		}

		const isActive = this.#isCurrentLocation;

		if (this._isActive.getValue() === isActive) return;
		if (!this.entityType || this.unique === undefined) {
			throw new Error('Could not check active state, entity type or unique is missing');
		}

		const ascending = this.getAscending();
		// Only if this type of item has ancestors...
		if (ascending) {
			const path = [...ascending, { entityType: this.entityType, unique: this.unique }];

			await this.#gotTreeContext;

			if (isActive) {
				this._treeContext?.activeManager.setActiveTrail(path);
			} else {
				// If this is the current, then remove it:
				// This is a hack, where we are assuming that another active item would have made its entrance and replaced the 'active' within 2 second. [NL]
				// The problem is that it may take some time before an item appears in the tree and communicates that its active.
				// And in the meantime the removal of this would have resulted in the parent closing. And since we don't use Active state to open the tree, then we have a problem.
				debounce(() => this._treeContext?.activeManager.removeActiveTrailIfMatch(path), 1000);
			}
		}
		this._isActive.setValue(isActive);
	};

	open(): void {
		const item = this.getTreeItem();
		if (!item) return;
		this._treeContext?.open?.(item);
	}

	select(): void {
		if (this.unique === undefined) throw new Error('Could not select. Unique is missing');
		this._treeContext?.selection.select(this.unique);
	}

	deselect(): void {
		if (this.unique === undefined) throw new Error('Could not deselect. Unique is missing');
		this._treeContext?.selection.deselect(this.unique);
	}

	constructPath(pathname: string, entityType: string, unique: string | null): string {
		return UMB_WORKSPACE_EDIT_PATH_PATTERN.generateAbsolute({
			sectionName: pathname,
			entityType,
			unique: unique ?? 'null',
		});
	}
}
