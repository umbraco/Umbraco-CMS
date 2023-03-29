import { map } from 'rxjs';
import {
	UmbSectionSidebarContext,
	UMB_SECTION_SIDEBAR_CONTEXT_TOKEN,
} from '../../section/section-sidebar/section-sidebar.context';
import { UmbSectionContext, UMB_SECTION_CONTEXT_TOKEN } from '../../section/section.context';
import { UmbTreeContextBase } from '../tree.context';
import { UmbTreeItemContext } from '../tree-item.context.interface';
import { ManifestEntityAction } from '@umbraco-cms/backoffice/extensions-registry';
import { BooleanState, DeepState, StringState, UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import {
	UmbContextConsumerController,
	UmbContextProviderController,
	UmbContextToken,
} from '@umbraco-cms/backoffice/context-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extensions-api';
import type { TreeItemPresentationModel } from '@umbraco-cms/backoffice/backend-api';

// add type for unique function
export type UmbTreeItemUniqueFunction<T extends TreeItemPresentationModel> = (x: T) => string | null | undefined;

export class UmbTreeItemContextBase<T extends TreeItemPresentationModel = TreeItemPresentationModel>
	implements UmbTreeItemContext<T>
{
	public host: UmbControllerHostElement;
	public unique?: string;
	public type?: string;

	#treeItem = new DeepState<T | undefined>(undefined);
	treeItem = this.#treeItem.asObservable();

	#hasChildren = new BooleanState(false);
	hasChildren = this.#hasChildren.asObservable();

	#isLoading = new BooleanState(false);
	isLoading = this.#isLoading.asObservable();

	#isSelectable = new BooleanState(false);
	isSelectable = this.#isSelectable.asObservable();

	#isSelected = new BooleanState(false);
	isSelected = this.#isSelected.asObservable();

	#isActive = new BooleanState(false);
	isActive = this.#isActive.asObservable();

	#hasActions = new BooleanState(false);
	hasActions = this.#hasActions.asObservable();

	#path = new StringState('');
	path = this.#path.asObservable();

	treeContext?: UmbTreeContextBase;
	#sectionContext?: UmbSectionContext;
	#sectionSidebarContext?: UmbSectionSidebarContext;
	#getUniqueFunction: UmbTreeItemUniqueFunction<T>;
	#actionObserver?: UmbObserverController<ManifestEntityAction[]>;

	constructor(host: UmbControllerHostElement, getUniqueFunction: UmbTreeItemUniqueFunction<T>) {
		this.host = host;
		this.#getUniqueFunction = getUniqueFunction;
		this.#consumeContexts();
		new UmbContextProviderController(host, UMB_TREE_ITEM_CONTEXT_TOKEN, this);
	}

	public setTreeItem(treeItem: T | undefined) {
		if (!treeItem) {
			this.#treeItem.next(undefined);
			return;
		}

		const unique = this.#getUniqueFunction(treeItem);
		if (!unique) throw new Error('Could not create tree item context, unique key is missing');
		this.unique = unique;

		if (!treeItem.type) throw new Error('Could not create tree item context, tree item type is missing');
		this.type = treeItem.type;

		this.#hasChildren.next(treeItem.hasChildren || false);
		this.#observeActions();
		this.#treeItem.next(treeItem);
	}

	public async requestChildren() {
		if (!this.unique) throw new Error('Could not request children, unique key is missing');

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
		if (!this.unique) throw new Error('Could not request children, unique key is missing');
		this.treeContext?.select(this.unique);
	}

	public deselect() {
		if (!this.unique) throw new Error('Could not request children, unique key is missing');
		this.treeContext?.deselect(this.unique);
	}

	#consumeContexts() {
		new UmbContextConsumerController(this.host, UMB_SECTION_CONTEXT_TOKEN, (instance) => {
			this.#sectionContext = instance;
			this.#observeSectionPath();
		});

		new UmbContextConsumerController(this.host, UMB_SECTION_SIDEBAR_CONTEXT_TOKEN, (instance) => {
			this.#sectionSidebarContext = instance;
		});

		new UmbContextConsumerController(this.host, 'umbTreeContext', (treeContext: UmbTreeContextBase) => {
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
		new UmbObserverController(this.host, this.treeContext.selectable, (value) => this.#isSelectable.next(value));
	}

	#observeIsSelected() {
		if (!this.treeContext) throw new Error('Could not request children, tree context is missing');
		if (!this.unique) throw new Error('Could not request children, unique key is missing');

		new UmbObserverController(
			this.host,
			this.treeContext.selection.pipe(map((selection) => selection.includes(this.unique!))),
			(isSelected) => {
				this.#isSelected.next(isSelected);
			}
		);
	}

	#observeSectionPath() {
		if (!this.#sectionContext) return;

		new UmbObserverController(this.host, this.#sectionContext.pathname, (pathname) => {
			if (!pathname) return;
			if (!this.type) throw new Error('Cant construct path, entity type is missing');
			if (!this.unique) throw new Error('Cant construct path, unique is missing');

			const path = this.constructPath(pathname, this.type, this.unique);
			this.#path.next(path);
		});
	}

	#observeActions() {
		if (this.#actionObserver) this.#actionObserver.destroy();

		this.#actionObserver = new UmbObserverController(
			this.host,
			umbExtensionsRegistry
				.extensionsOfType('entityAction')
				.pipe(map((actions) => actions.filter((action) => action.conditions.entityType === this.type))),
			(actions) => {
				this.#hasActions.next(actions.length > 0);
			}
		);
	}

	// TODO: use router context
	constructPath(pathname: string, entityType: string, unique: string) {
		return `section/${pathname}/workspace/${entityType}/edit/${unique}`;
	}
}

export const UMB_TREE_ITEM_CONTEXT_TOKEN = new UmbContextToken<UmbTreeItemContext>('UmbTreeItemContext');
