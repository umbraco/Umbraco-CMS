import type { UmbTreeItemModel, UmbTreeRootModel } from '../types.js';
import { map } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';

export class UmbTreeItemEntityActionManager<
	TreeItemType extends UmbTreeItemModel = UmbTreeItemModel,
	TreeRootType extends UmbTreeRootModel = UmbTreeRootModel,
> extends UmbControllerBase {
	#hasActions = new UmbBooleanState(false);
	readonly hasActions = this.#hasActions.asObservable();

	#treeItem: TreeItemType | TreeRootType | undefined;
	#observerController?: UmbObserverController;

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
		this.#observeActionsForTreeItem(this.#treeItem);
	}

	/**
	 * Gets the tree item for which to load children.
	 * @returns {TreeItemType | TreeRootType | undefined} - The tree item for the children
	 * @memberof UmbTreeItemChildrenManager
	 */
	public getTreeItem(): TreeItemType | TreeRootType | undefined {
		return this.#treeItem;
	}

	#observeActionsForTreeItem(treeItem: TreeItemType | TreeRootType) {
		this.#observerController = this.observe(
			umbExtensionsRegistry
				.byType('entityAction')
				.pipe(map((actions) => actions.filter((action) => action.forEntityTypes.includes(treeItem.entityType)))),
			(actions) => {
				this.#hasActions.setValue(actions.length > 0);
			},
			'observeActions',
		);
	}
}
