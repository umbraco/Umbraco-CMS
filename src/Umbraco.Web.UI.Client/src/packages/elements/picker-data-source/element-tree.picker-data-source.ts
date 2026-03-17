import { UmbElementFolderItemRepository } from '../folder/repository/item/element-folder-item.repository.js';
import { UmbElementItemRepository } from '../item/repository/element-item.repository.js';
import { UmbElementTreeRepository } from '../tree/element-tree.repository.js';
import { getConfigValue } from '@umbraco-cms/backoffice/utils';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbPickerTreeDataSource } from '@umbraco-cms/backoffice/picker-data-source';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeItemModel,
	UmbTreeRootItemsRequestArgs,
	UmbTreeStartNode,
} from '@umbraco-cms/backoffice/tree';
import type { UmbConfigCollectionModel } from '@umbraco-cms/backoffice/utils';

export class UmbElementTreePickerDataSource extends UmbControllerBase implements UmbPickerTreeDataSource {
	#elementItem = new UmbElementItemRepository(this);
	#folderItem = new UmbElementFolderItemRepository(this);
	#folderOnly = false;
	#startNode?: UmbTreeStartNode;
	#tree = new UmbElementTreeRepository(this);

	setConfig(config: UmbConfigCollectionModel | undefined) {
		this.#folderOnly = Boolean(getConfigValue(config, 'folderOnly'));
		this.#startNode = getConfigValue(config, 'startNode');
		console.log('startNode', this.#startNode);
	}

	requestTreeRoot = () => this.#tree.requestTreeRoot();

	requestTreeRootItems = (args: UmbTreeRootItemsRequestArgs) => {
		args.foldersOnly = this.#folderOnly;
		return this.#tree.requestTreeRootItems(args);
	};

	requestTreeItemsOf = (args: UmbTreeChildrenOfRequestArgs) => {
		args.foldersOnly = this.#folderOnly;
		return this.#tree.requestTreeItemsOf(args);
	};

	requestTreeItemAncestors = (args: UmbTreeAncestorsOfRequestArgs) => this.#tree.requestTreeItemAncestors(args);

	requestItems = (uniques: Array<string>) =>
		this.#folderOnly ? this.#folderItem.requestItems(uniques) : this.#elementItem.requestItems(uniques);

	treePickableFilter: (treeItem: UmbTreeItemModel) => boolean = (treeItem) => treeItem.isFolder === this.#folderOnly;
}
