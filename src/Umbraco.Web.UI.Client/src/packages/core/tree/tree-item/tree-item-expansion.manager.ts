import { UMB_TREE_CONTEXT } from '../tree.context.token.js';
import type { UmbTreeItemModel, UmbTreeRootModel } from '../types.js';
import type { UmbTreeItemChildrenManager } from './tree-item-children.manager.js';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import type { UmbTargetPaginationManager } from '@umbraco-cms/backoffice/utils';

interface UmbTreeItemTargetExpansionManagerArgs<
	TreeItemType extends UmbTreeItemModel = UmbTreeItemModel,
	TreeRootType extends UmbTreeRootModel = UmbTreeRootModel,
> {
	childrenManager: UmbTreeItemChildrenManager<TreeItemType, TreeRootType>;
	targetPaginationManager: UmbTargetPaginationManager;
}

export class UmbTreeItemTargetExpansionManager<
	TreeItemType extends UmbTreeItemModel = UmbTreeItemModel,
	TreeRootType extends UmbTreeRootModel = UmbTreeRootModel,
> extends UmbControllerBase {
	#isExpanded = new UmbBooleanState(false);
	isExpanded = this.#isExpanded.asObservable();

	#treeItem: TreeItemType | TreeRootType | undefined;
	#observerController: UmbObserverController | undefined;
	#init?: Promise<unknown>;
	#childrenManager;
	#targetPaginationManager;
	#treeContext?: typeof UMB_TREE_CONTEXT.TYPE;

	constructor(host: UmbControllerHost, args: UmbTreeItemTargetExpansionManagerArgs<TreeItemType, TreeRootType>) {
		super(host);
		this.#childrenManager = args.childrenManager;
		this.#targetPaginationManager = args.targetPaginationManager;

		this.#init = Promise.all([
			this.consumeContext(UMB_TREE_CONTEXT, (treeContext) => {
				this.#treeContext = treeContext;
			}).asPromise(),
		]);
	}

	/**
	 * Set the parent for which to load children.
	 * @param {(TreeItemType | TreeRootType | undefined)} treeItem - The tree item model
	 * @memberof UmbTreeItemChildrenManager
	 */
	public setTreeItem(treeItem: TreeItemType | TreeRootType | undefined) {
		// If we don't get a tree item stop the observation
		if (!treeItem) {
			this.#observerController?.destroy();
			return;
		}

		// If we get the same tree item again, continue with the same observation
		if (treeItem.entityType === this.#treeItem?.entityType && treeItem.unique === this.#treeItem?.unique) {
			return;
		}

		this.#treeItem = treeItem;
		this.#observeExpansionForTreeItem(this.#treeItem);
	}

	/**
	 * Gets the tree item for which to load children.
	 * @returns {TreeItemType | TreeRootType | undefined} - The tree item for the children
	 * @memberof UmbTreeItemChildrenManager
	 */
	public getTreeItem(): TreeItemType | TreeRootType | undefined {
		return this.#treeItem;
	}

	async #observeExpansionForTreeItem(treeItem: TreeItemType | TreeRootType) {
		await this.#init;

		if (!this.#treeContext) {
			throw new Error('Tree context is not set');
		}

		this.#observerController = this.observe(
			this.#treeContext?.expansion.entry(treeItem),
			async (entry) => {
				const isExpanded = entry !== undefined;
				this.#isExpanded.setValue(isExpanded);

				const currentBaseTarget = this.#targetPaginationManager.getBaseTarget();
				const target = entry?.target;

				/* If a base target already exists (tree loaded to that point),
          don’t auto-reset when the target is removed.
          This happens when creating new items not yet in the tree. */
				if (currentBaseTarget && !target) {
					return;
				}

				/* If a new target is set we only want to reload children if the new target isn’t among the already loaded items. */
				const targetIsLoaded = this.#childrenManager.isChildLoaded(target);
				if (target && targetIsLoaded) {
					return;
				}

				// If we already have children and the target didn't change then we don't have to load new children
				const isNewTarget = target !== currentBaseTarget;
				if (isExpanded && this.#childrenManager.hasLoadedChildren() && !isNewTarget) {
					return;
				}

				// If this item is expanded and has children, load them
				if (isExpanded && this.#childrenManager.getHasChildren()) {
					this.#targetPaginationManager.setBaseTarget(target);
					this.#childrenManager.loadChildren();
				}
			},
			'observeExpansion',
		);
	}
}
