import { UmbElementTreeRepository } from '../../tree/element-tree.repository.js';
import { UmbElementItemRepository } from '../../item/repository/element-item.repository.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbPickerTreeDataSource } from '@umbraco-cms/backoffice/picker-data-source';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeItemModel,
	UmbTreeRootItemsRequestArgs,
} from '@umbraco-cms/backoffice/tree';

export class UmbElementFolderTreePropertyEditorDataSource extends UmbControllerBase implements UmbPickerTreeDataSource {
	#item = new UmbElementItemRepository(this); // TODO: [LK] Change to `UmbElementFolderItemRepository` once we have the endpoint.
	#tree = new UmbElementTreeRepository(this);

	requestTreeRoot = () => this.#tree.requestTreeRoot();

	requestTreeRootItems = (args: UmbTreeRootItemsRequestArgs) => {
		args.foldersOnly = true;
		return this.#tree.requestTreeRootItems(args);
	};

	requestTreeItemsOf = (args: UmbTreeChildrenOfRequestArgs) => {
		args.foldersOnly = true;
		return this.#tree.requestTreeItemsOf(args);
	};

	requestTreeItemAncestors = (args: UmbTreeAncestorsOfRequestArgs) => this.#tree.requestTreeItemAncestors(args);

	requestItems = (uniques: Array<string>) => this.#item.requestItems(uniques);

	treePickableFilter: (treeItem: UmbTreeItemModel) => boolean = (treeItem) => treeItem.isFolder;
}

export { UmbElementFolderTreePropertyEditorDataSource as api };
