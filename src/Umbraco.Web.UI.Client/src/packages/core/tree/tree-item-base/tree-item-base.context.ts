import { UmbTreeItemContext } from '../tree-item/tree-item.context.interface.js';
import { UmbTreeContextBase } from '../tree.context.js';
import { map } from '@umbraco-cms/backoffice/external/rxjs';
import { UMB_SECTION_CONTEXT_TOKEN, UMB_SECTION_SIDEBAR_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/section';
import type { UmbSectionContext, UmbSectionSidebarContext } from '@umbraco-cms/backoffice/section';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbBooleanState, UmbDeepState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UmbBaseController, UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { TreeItemPresentationModel } from '@umbraco-cms/backoffice/backend-api';

// add type for unique function
export type UmbTreeItemUniqueFunction<TreeItemType extends TreeItemPresentationModel> = (
	x: TreeItemType
) => string | null | undefined;

export class UmbTreeItemContextBase<TreeItemType extends TreeItemPresentationModel>
	extends UmbBaseController
	implements UmbTreeItemContext<TreeItemType>
{
	public unique?: string | null;
	public type?: string;

	#treeItem = new UmbDeepState<TreeItemType | undefined>(undefined);
	treeItem = this.#treeItem.asObservable();

	#hasChildren = new UmbBooleanState(false);
	hasChildren = this.#hasChildren.asObservable();

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
	#getUniqueFunction: UmbTreeItemUniqueFunction<TreeItemType>;

	constructor(host: UmbControllerHost, getUniqueFunction: UmbTreeItemUniqueFunction<TreeItemType>) {
		super(host);
		this.#getUniqueFunction = getUniqueFunction;
		this.#consumeContexts();
		this.provideContext(UMB_TREE_ITEM_CONTEXT_TOKEN, this);
	}

	public setTreeItem(treeItem: TreeItemType | undefined) {
		if (!treeItem) {
			this.#treeItem.next(undefined);
			return;
		}

		const unique = this.#getUniqueFunction(treeItem);
		// Only check for undefined. The tree root has null as unique
		if (unique === undefined) throw new Error('Could not create tree item context, unique key is missing');
		this.unique = unique;

		if (!treeItem.type) throw new Error('Could not create tree item context, tree item type is missing');
		this.type = treeItem.type;

		this.#hasChildren.next(treeItem.hasChildren || false);
		this.#treeItem.next(treeItem);

		// Update observers:
		this.#observeActions();
		this.#observeIsSelectable();
		this.#observeIsSelected();
		this.#observeSectionPath();
	}

	public async requestChildren() {
		if (this.unique === undefined) throw new Error('Could not request children, unique key is missing');

		// TODO: wait for tree context to be ready
		this.#isLoading.next(true);
		const response = await this.treeContext!.requestChildrenOf(this.unique);
		this.#isLoading.next(false);
		return response;
	}

	public toggleContextMenu() {
		if (!this.getTreeItem() || !this.type || this.unique === undefined) {
			throw new Error('Could not request children, tree item is not set');
		}

		this.#sectionSidebarContext?.toggleContextMenu(this.type, this.unique, this.getTreeItem()?.name || '');
	}

	public select() {
		if (this.unique === undefined) throw new Error('Could not request children, unique key is missing');
		this.treeContext?.select(this.unique);
	}

	public deselect() {
		if (this.unique === undefined) throw new Error('Could not request children, unique key is missing');
		this.treeContext?.deselect(this.unique);
	}

	#consumeContexts() {
		this.consumeContext(UMB_SECTION_CONTEXT_TOKEN, (instance) => {
			this.#sectionContext = instance;
			this.#observeSectionPath();
		});

		this.consumeContext(UMB_SECTION_SIDEBAR_CONTEXT_TOKEN, (instance) => {
			this.#sectionSidebarContext = instance;
		});

		this.consumeContext('umbTreeContext', (treeContext: UmbTreeContextBase<TreeItemType>) => {
			this.treeContext = treeContext;
			this.#observeIsSelectable();
			this.#observeIsSelected();
		});
	}

	getTreeItem() {
		return this.#treeItem.getValue();
	}

	#observeIsSelectable() {
		if (!this.treeContext) return;
		this.observe(
			this.treeContext.selectable,
			(value) => {
				this.#isSelectableContext.next(value);

				// If the tree is selectable, check if this item is selectable
				if (value === true) {
					const isSelectable = this.treeContext?.selectableFilter?.(this.getTreeItem()!) ?? true;
					this.#isSelectable.next(isSelectable);
				}
			},
			'observeIsSelectable'
		);
	}

	#observeIsSelected() {
		if (!this.treeContext || !this.unique) return;

		this.observe(
			this.treeContext.selection.pipe(map((selection) => selection.includes(this.unique!))),
			(isSelected) => {
				this.#isSelected.next(isSelected);
			},
			'observeIsSelected'
		);
	}

	#observeSectionPath() {
		if (!this.#sectionContext) return;

		this.observe(
			this.#sectionContext.pathname,
			(pathname) => {
				if (!pathname || !this.type || this.unique === undefined) return;
				const path = this.constructPath(pathname, this.type, this.unique);
				this.#path.next(path);
			},
			'observeSectionPath'
		);
	}

	#observeActions() {
		this.observe(
			umbExtensionsRegistry
				.extensionsOfType('entityAction')
				.pipe(map((actions) => actions.filter((action) => action.meta.entityTypes.includes(this.type!)))),
			(actions) => {
				this.#hasActions.next(actions.length > 0);
			},
			'observeActions'
		);
	}

	// TODO: use router context
	constructPath(pathname: string, entityType: string, unique: string | null) {
		return `section/${pathname}/workspace/${entityType}/edit/${unique}`;
	}
}

export const UMB_TREE_ITEM_CONTEXT_TOKEN = new UmbContextToken<UmbTreeItemContext<any>>('UmbTreeItemContext');
