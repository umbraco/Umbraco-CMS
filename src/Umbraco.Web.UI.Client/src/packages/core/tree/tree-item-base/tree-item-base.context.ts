import type { UmbTreeItemContext } from '../tree-item-default/tree-item.context.interface.js';
import type { UmbTreeContextBase } from '../tree.context.js';
import type { UmbTreeItemModelBase } from '../types.js';
import { UmbReloadTreeItemChildrenRequestEntityActionEvent } from '../reload-tree-item-children/index.js';
import { map } from '@umbraco-cms/backoffice/external/rxjs';
import { UMB_SECTION_CONTEXT, UMB_SECTION_SIDEBAR_CONTEXT } from '@umbraco-cms/backoffice/section';
import type { UmbSectionContext, UmbSectionSidebarContext } from '@umbraco-cms/backoffice/section';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbBooleanState, UmbDeepState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UMB_ACTION_EVENT_CONTEXT, type UmbActionEventContext } from '@umbraco-cms/backoffice/action';
import type { UmbEntityActionEvent } from '@umbraco-cms/backoffice/entity-action';

export type UmbTreeItemUniqueFunction<TreeItemType extends UmbTreeItemModelBase> = (
	x: TreeItemType,
) => string | null | undefined;

export class UmbTreeItemContextBase<TreeItemType extends UmbTreeItemModelBase>
	extends UmbBaseController
	implements UmbTreeItemContext<TreeItemType>
{
	public unique?: string | null;
	public entityType?: string;

	#treeItem = new UmbDeepState<TreeItemType | undefined>(undefined);
	treeItem = this.#treeItem.asObservable();

	#hasChildren = new UmbBooleanState(false);
	hasChildren = this.#hasChildren.asObservable();
	#hasChildrenInitValueFlag = false;

	#isLoading = new UmbBooleanState(false);
	isLoading = this.#isLoading.asObservable();

	#isSelectable = new UmbBooleanState(false);
	isSelectable = this.#isSelectable.asObservable();

	#isSelectableContext = new UmbBooleanState(false);
	isSelectableContext = this.#isSelectableContext.asObservable();

	#isSelected = new UmbBooleanState(false);
	isSelected = this.#isSelected.asObservable();

	#isActive = new UmbBooleanState(false);
	isActive = this.#isActive.asObservable();

	#hasActions = new UmbBooleanState(false);
	hasActions = this.#hasActions.asObservable();

	#path = new UmbStringState('');
	path = this.#path.asObservable();

	treeContext?: UmbTreeContextBase<TreeItemType>;
	#sectionContext?: UmbSectionContext;
	#sectionSidebarContext?: UmbSectionSidebarContext;
	#actionEventContext?: UmbActionEventContext;
	#getUniqueFunction: UmbTreeItemUniqueFunction<TreeItemType>;

	constructor(host: UmbControllerHost, getUniqueFunction: UmbTreeItemUniqueFunction<TreeItemType>) {
		super(host);
		this.#getUniqueFunction = getUniqueFunction;
		this.#consumeContexts();
		this.provideContext(UMB_TREE_ITEM_CONTEXT, this);
	}

	public setTreeItem(treeItem: TreeItemType | undefined) {
		if (!treeItem) {
			this.#treeItem.setValue(undefined);
			return;
		}

		const unique = this.#getUniqueFunction(treeItem);
		// Only check for undefined. The tree root has null as unique
		if (unique === undefined) throw new Error('Could not create tree item context, unique key is missing');
		this.unique = unique;

		if (!treeItem.entityType) throw new Error('Could not create tree item context, tree item type is missing');
		this.entityType = treeItem.entityType;

		this.#hasChildren.setValue(treeItem.hasChildren || false);
		this.#treeItem.setValue(treeItem);

		// Update observers:
		this.#observeActions();
		this.#observeIsSelectable();
		this.#observeIsSelected();
		this.#observeSectionPath();
	}

	public async requestChildren() {
		if (this.unique === undefined) throw new Error('Could not request children, unique key is missing');

		// TODO: wait for tree context to be ready
		this.#isLoading.setValue(true);
		const response = await this.treeContext!.requestChildrenOf(this.unique);
		this.#isLoading.setValue(false);
		return response;
	}

	public toggleContextMenu() {
		if (!this.getTreeItem() || !this.entityType || this.unique === undefined) {
			throw new Error('Could not request children, tree item is not set');
		}

		this.#sectionSidebarContext?.toggleContextMenu(this.entityType, this.unique, this.getTreeItem()?.name || '');
	}

	public select() {
		if (this.unique === undefined) throw new Error('Could not select. Unique is missing');
		this.treeContext?.selection.select(this.unique);
	}

	public deselect() {
		if (this.unique === undefined) throw new Error('Could not deselect. Unique is missing');
		this.treeContext?.selection.deselect(this.unique);
	}

	#consumeContexts() {
		this.consumeContext(UMB_SECTION_CONTEXT, (instance) => {
			this.#sectionContext = instance;
			this.#observeSectionPath();
		});

		this.consumeContext(UMB_SECTION_SIDEBAR_CONTEXT, (instance) => {
			this.#sectionSidebarContext = instance;
		});

		this.consumeContext('umbTreeContext', (treeContext: UmbTreeContextBase<TreeItemType>) => {
			this.treeContext = treeContext;
			this.#observeIsSelectable();
			this.#observeIsSelected();
			this.#observeHasChildren();
		});

		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (instance) => {
			this.#actionEventContext = instance;
			this.#actionEventContext.removeEventListener(
				UmbReloadTreeItemChildrenRequestEntityActionEvent.TYPE,
				this.#onReloadRequest as EventListener,
			);
			this.#actionEventContext.addEventListener(
				UmbReloadTreeItemChildrenRequestEntityActionEvent.TYPE,
				this.#onReloadRequest as EventListener,
			);
		});
	}

	getTreeItem() {
		return this.#treeItem.getValue();
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

	#observeSectionPath() {
		if (!this.#sectionContext) return;

		this.observe(
			this.#sectionContext.pathname,
			(pathname) => {
				if (!pathname || !this.entityType || this.unique === undefined) return;
				const path = this.constructPath(pathname, this.entityType, this.unique);
				this.#path.setValue(path);
			},
			'observeSectionPath',
		);
	}

	#observeActions() {
		this.observe(
			umbExtensionsRegistry
				.byType('entityAction')
				.pipe(map((actions) => actions.filter((action) => action.meta.entityTypes.includes(this.entityType!)))),
			(actions) => {
				this.#hasActions.setValue(actions.length > 0);
			},
			'observeActions',
		);
	}

	async #observeHasChildren() {
		if (!this.treeContext || !this.unique) return;

		const observable = await this.treeContext.childrenOf(this.unique);

		// observe if any children will be added runtime to a tree item. Nested items/folders etc.
		this.observe(observable.pipe(map((children) => children.length > 0)), (hasChildren) => {
			// we need to skip the first value, because it will also return false until a child is in the store
			// we therefor rely on the value from the tree item itself
			if (this.#hasChildrenInitValueFlag === true) {
				this.#hasChildren.setValue(hasChildren);
			}
			this.#hasChildrenInitValueFlag = true;
		});
	}

	#onReloadRequest = (event: UmbEntityActionEvent) => {
		// Only handle children request here. Root request is handled by the tree context
		if (!this.unique) return;
		if (event.getUnique() !== this.unique) return;
		if (event.getEntityType() !== this.entityType) return;
		this.requestChildren();
	};

	// TODO: use router context
	constructPath(pathname: string, entityType: string, unique: string | null) {
		return `section/${pathname}/workspace/${entityType}/edit/${unique}`;
	}

	destroy(): void {
		this.#actionEventContext?.removeEventListener(
			UmbReloadTreeItemChildrenRequestEntityActionEvent.TYPE,
			this.#onReloadRequest as EventListener,
		);
		super.destroy();
	}
}

export const UMB_TREE_ITEM_CONTEXT = new UmbContextToken<UmbTreeItemContext<any>>('UmbTreeItemContext');
