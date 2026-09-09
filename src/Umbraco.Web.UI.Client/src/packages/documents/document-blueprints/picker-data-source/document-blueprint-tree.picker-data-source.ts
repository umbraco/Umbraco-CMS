import { UmbDocumentBlueprintItemRepository } from '../repository/item/document-blueprint-item.repository.js';
import { UmbDocumentBlueprintFolderItemRepository } from '../tree/folder/repository/item/document-blueprint-folder-item.repository.js';
import { UmbDocumentBlueprintTreeRepository } from '../tree/document-blueprint-tree.repository.js';
import { getConfigValue } from '@umbraco-cms/backoffice/utils';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeItemModel,
	UmbTreeRootItemsRequestArgs,
	UmbTreeStartNode,
} from '@umbraco-cms/backoffice/tree';
import type { UmbConfigCollectionModel } from '@umbraco-cms/backoffice/utils';
import type { UmbPickerTreeDataSource } from '@umbraco-cms/backoffice/picker-data-source';

export class UmbDocumentBlueprintTreePickerDataSource extends UmbControllerBase implements UmbPickerTreeDataSource {
	#documentBlueprintItem = new UmbDocumentBlueprintItemRepository(this);
	#folderItem = new UmbDocumentBlueprintFolderItemRepository(this);
	#folderOnly = false;
	#tree = new UmbDocumentBlueprintTreeRepository(this);
	#startNode?: UmbTreeStartNode;

	setConfig(config: UmbConfigCollectionModel | undefined) {
		this.#folderOnly = Boolean(getConfigValue(config, 'folderOnly'));
		this.#startNode = getConfigValue(config, 'startNode');
	}

	async requestTreeStartNode() {
		return this.#startNode ?? undefined;
	}

	requestTreeRoot() {
		return this.#tree.requestTreeRoot();
	}

	requestTreeRootItems(args: UmbTreeRootItemsRequestArgs) {
		return this.#tree.requestTreeRootItems({ ...args, foldersOnly: this.#folderOnly });
	}

	requestTreeItemsOf(args: UmbTreeChildrenOfRequestArgs) {
		return this.#tree.requestTreeItemsOf({ ...args, foldersOnly: this.#folderOnly });
	}

	requestTreeItemAncestors(args: UmbTreeAncestorsOfRequestArgs) {
		return this.#tree.requestTreeItemAncestors(args);
	}

	requestItems(uniques: Array<string>) {
		return this.#folderOnly
			? this.#folderItem.requestItems(uniques)
			: this.#documentBlueprintItem.requestItems(uniques);
	}

	treePickableFilter = (treeItem: UmbTreeItemModel): boolean => treeItem.isFolder === this.#folderOnly;
}
