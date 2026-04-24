import type { ManifestTreeItem } from '../../extensions/types.js';
import type { UmbTreeItemContext } from '../tree-item-context.interface.js';
import type { UmbTreeItemModel, UmbTreeRootModel } from '../../types.js';
import { UmbTreeItemChildrenManager } from '../tree-item-children.manager.js';
import { UmbTreeItemTargetExpansionManager } from '../tree-item-expansion.manager.js';
import type { UMB_TREE_CONTEXT } from '../../tree.context.token.js';
import { UmbTreeItemApiBase } from './tree-item-api-base.js';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export abstract class UmbTreeItemContextBase<
		TreeItemType extends UmbTreeItemModel,
		TreeRootType extends UmbTreeRootModel,
		ManifestType extends ManifestTreeItem = ManifestTreeItem,
	>
	extends UmbTreeItemApiBase<TreeItemType, ManifestType>
	implements UmbTreeItemContext<TreeItemType>
{
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

	#isMenu = false;
	setIsMenu(isMenu: boolean) {
		this.#isMenu = isMenu;
	}
	getIsMenu() {
		return this.#isMenu;
	}

	constructor(host: UmbControllerHost) {
		super(host);
		// TODO: Get take size from Tree context
		this._treeItemChildrenManager.setTakeSize(50);
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
		return this.manifest;
	}

	public override setTreeItem(treeItem: TreeItemType | undefined) {
		super.setTreeItem(treeItem);
		if (!treeItem) return;
		this._treeItemChildrenManager.setTreeItem(treeItem);
		this.#treeItemExpansionManager.setTreeItem(treeItem);
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
		this._treeContext?.expansion.expandItem({ entityType, unique });
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

		this._treeContext?.expansion.collapseItem({ entityType, unique });
	}

	protected override _onTreeContextChanged(context: typeof UMB_TREE_CONTEXT.TYPE): void {
		super._onTreeContextChanged(context);
		this.#observeFoldersOnly();
		this.#observeAdditionalRequestArgs();
	}

	#observeFoldersOnly() {
		if (this.unique === undefined) return;

		this.observe(
			this._treeContext?.foldersOnly,
			(foldersOnly) => {
				this._treeItemChildrenManager.setFoldersOnly(foldersOnly ?? false);
			},
			'observeFoldersOnly',
		);
	}

	#observeAdditionalRequestArgs() {
		if (this.unique === undefined) return;

		this.observe(
			this._treeContext?.additionalRequestArgs,
			(additionalRequestArgs) => {
				if (!additionalRequestArgs) return;
				this._treeItemChildrenManager.setAdditionalRequestArgs(additionalRequestArgs);
			},
			'observeAdditionalRequestArgs',
		);
	}
}
