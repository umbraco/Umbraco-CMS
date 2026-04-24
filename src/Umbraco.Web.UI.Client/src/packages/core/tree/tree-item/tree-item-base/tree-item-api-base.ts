import type { UmbTreeItemModel } from '../../types.js';
import { UMB_TREE_CONTEXT } from '../../tree.context.token.js';
import { UMB_TREE_ITEM_API_CONTEXT } from '../tree-item.context.token.js';
import { UmbTreeItemEntityActionManager } from '../tree-item-entity-action.manager.js';
import { map } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbBooleanState, UmbObjectState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_WORKSPACE_EDIT_PATH_PATTERN } from '@umbraco-cms/backoffice/workspace';
import { ensureSlash } from '@umbraco-cms/backoffice/router';
import { debounce } from '@umbraco-cms/backoffice/utils';
import { UmbEntityContext, UmbParentEntityContext } from '@umbraco-cms/backoffice/entity';
import { UMB_SECTION_CONTEXT } from '@umbraco-cms/backoffice/section';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityModel, UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbContextMinimal } from '@umbraco-cms/backoffice/context-api';
import type { ManifestBase, UmbApi } from '@umbraco-cms/backoffice/extension-api';

/**
 * Shared contract for tree item contexts and card apis covering item data,
 * selection, active state, path, and entity actions. Children, expansion, and
 * pagination are not included here and remain exclusively on `UmbTreeItemContextBase`.
 */
export interface UmbTreeItemApi<
	TreeItemType extends UmbTreeItemModel = UmbTreeItemModel,
	ManifestType extends ManifestBase = ManifestBase,
> extends UmbApi,
		UmbContextMinimal {
	unique?: UmbEntityUnique;
	entityType?: string;
	manifest: ManifestType | undefined;
	readonly treeItem: Observable<TreeItemType | undefined>;
	readonly isSelectable: Observable<boolean>;
	readonly isSelectableContext: Observable<boolean>;
	readonly isSelected: Observable<boolean>;
	readonly isActive: Observable<boolean>;
	readonly hasActions: Observable<boolean>;
	readonly path: Observable<string>;
	setTreeItem(item: TreeItemType | undefined): void;
	getTreeItem(): TreeItemType | undefined;
	open(): void;
	select(): void;
	deselect(): void;
	constructPath(pathname: string, entityType: string, unique: string | null): string;
}

/**
 * Abstract base for tree item apis. Handles item data, selection, active state,
 * path, and entity actions — without children, expansion, or pagination.
 *
 * Provides itself as `UMB_TREE_ITEM_API_CONTEXT` so entity action conditions
 * can discover a tree item regardless of which tree view is active.
 */
export abstract class UmbTreeItemApiBase<
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

	#hasActiveDescendant = new UmbBooleanState(undefined);
	readonly hasActiveDescendant = this.#hasActiveDescendant.asObservable();

	#treeItemEntityActionManager = new UmbTreeItemEntityActionManager(this);
	readonly hasActions = this.#treeItemEntityActionManager.hasActions;

	protected readonly _selectOnly = new UmbBooleanState(false);
	readonly selectOnly = this._selectOnly.asObservable();

	#path = new UmbStringState('');
	readonly path = this.#path.asObservable();

	#sectionContext?: typeof UMB_SECTION_CONTEXT.TYPE;
	#entityContext = new UmbEntityContext(this);
	#parentContext = new UmbParentEntityContext(this);

	/** Public accessor for the tree context. Kept public for backward compatibility. */
	public get treeContext(): typeof UMB_TREE_CONTEXT.TYPE | undefined {
		return this._treeContext;
	}

	constructor(host: UmbControllerHost) {
		super(host, UMB_TREE_ITEM_API_CONTEXT);

		this._treeContextConsumer = this.consumeContext(UMB_TREE_CONTEXT, (context) => {
			this._treeContext = context;
			this._observeIsSelectable();
			this._observeIsSelected();
			this._observeSelectOnly();
			if (context) this._onTreeContextChanged(context);
		});
		this.#gotTreeContext = this._treeContextConsumer.asPromise();

		this.consumeContext(UMB_SECTION_CONTEXT, (instance) => {
			this.#sectionContext = instance;
			this.#observeSectionPath();
		});

		window.addEventListener('navigationend', this.#debouncedCheckIsActive);
	}

	setTreeItem(item: TreeItemType | undefined): void {
		if (!item) {
			this._treeItem.setValue(undefined);
			this.#entityContext.setEntityType(undefined);
			this.#entityContext.setUnique(null);
			this.#treeItemEntityActionManager.setTreeItem(undefined);
			return;
		}

		// Only check for undefined. The tree root has null as unique.
		if (item.unique === undefined) throw new Error('Could not set tree item, unique is missing');
		if (!item.entityType) throw new Error('Could not set tree item, entity type is missing');

		this._treeItem.setValue(item);
		this.unique = item.unique;
		this.entityType = item.entityType;

		this.#entityContext.setEntityType(item.entityType);
		this.#entityContext.setUnique(item.unique);

		const parentEntity: UmbEntityModel | undefined = item.parent
			? { entityType: item.parent.entityType, unique: item.parent.unique }
			: undefined;
		this.#parentContext.setParent(parentEntity);

		this.#treeItemEntityActionManager.setTreeItem(item);

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
					this.#checkIsActive();
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

	protected _observeSelectOnly() {
		const ctx = this._treeContext;
		if (!ctx) return;
		this.observe(ctx.selectOnly, (value) => this._selectOnly.setValue(value ?? false), '_observeSelectOnly');
	}

	/**
	 * Hook called when the tree context is received or changes. Subclasses can override to add additional observations.
	 * @param _context
	 */
	protected _onTreeContextChanged(_context: typeof UMB_TREE_CONTEXT.TYPE): void {
		this.#observeActive();
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
		const isSelectable = this._isSelectable.getValue();

		if (isSelectable) {
			this._isActive.setValue(false);
			return;
		}

		/* Check if the current location includes the path of this tree item.
		We ensure that the paths ends with a slash to avoid collisions with paths like /path-1 and /path-1-2 where /path-1 is in both.
		Instead we compare /path-1/ with /path-1-2/ which wont collide.*/
		const location = ensureSlash(window.location.pathname);
		const comparePath = ensureSlash(this.#path.getValue());
		const isActive = location.includes(comparePath);

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
				this._treeContext?.activeManager.setActive(path);
			} else {
				// If this is the current, then remove it:
				// This is a hack, where we are assuming that another active item would have made its entrance and replaced the 'active' within 2 second. [NL]
				// The problem is that it may take some time before an item appears in the tree and communicates that its active.
				// And in the meantime the removal of this would have resulted in the parent closing. And since we don't use Active state to open the tree, then we have a problem.
				debounce(() => this._treeContext?.activeManager.removeActiveIfMatch(path), 1000);
			}
		}
		this._isActive.setValue(isActive);
	};

	#debouncedCheckIsActive = debounce(this.#checkIsActive, 100);

	open(): void {
		const item = this.getTreeItem();
		if (!item) return;
		this._treeContext?.open(item);
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

	override destroy(): void {
		window.removeEventListener('navigationend', this.#debouncedCheckIsActive);
		super.destroy();
	}
}
