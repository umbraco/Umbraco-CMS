import { UmbElementFolderItemRepository } from '../folder/repository/item/element-folder-item.repository.js';
import { UmbElementItemRepository } from '../item/repository/element-item.repository.js';
import { UmbElementTreeRepository } from '../tree/element-tree.repository.js';
import type { UmbElementTreeChildrenOfRequestArgs, UmbElementTreeRootItemsRequestArgs } from '../tree/types.js';
import { getConfigValue } from '@umbraco-cms/backoffice/utils';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_PROPERTY_TYPE_BASED_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/content';
import type { UmbPickerTreeDataSource } from '@umbraco-cms/backoffice/picker-data-source';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeItemModel,
	UmbTreeRootItemsRequestArgs,
	UmbTreeStartNode,
} from '@umbraco-cms/backoffice/tree';
import type { UmbConfigCollectionModel } from '@umbraco-cms/backoffice/utils';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbElementTreePickerDataSource extends UmbControllerBase implements UmbPickerTreeDataSource {
	#elementItem = new UmbElementItemRepository(this);
	#folderItem = new UmbElementFolderItemRepository(this);
	#folderOnly = false;
	#startNode?: UmbTreeStartNode;
	#tree = new UmbElementTreeRepository(this);
	#dataType?: { unique: string };

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_PROPERTY_TYPE_BASED_PROPERTY_CONTEXT, (context) => {
			this.observe(context?.dataType, (dataType) => {
				this.#dataType = dataType;
			});
		});
	}

	setConfig(config: UmbConfigCollectionModel | undefined) {
		this.#folderOnly = Boolean(getConfigValue(config, 'folderOnly'));
		this.#startNode = getConfigValue(config, 'startNode');
		console.log('setConfig', this.#startNode);
	}

	requestTreeRoot = () => this.#tree.requestTreeRoot();

	requestTreeRootItems = (args: UmbTreeRootItemsRequestArgs) => {
		const typedArgs: UmbElementTreeRootItemsRequestArgs = {
			...args,
			foldersOnly: this.#folderOnly,
			dataType: this.#dataType,
		};
		return this.#tree.requestTreeRootItems(typedArgs);
	};

	requestTreeItemsOf = (args: UmbTreeChildrenOfRequestArgs) => {
		console.log('requestTreeItemsOf', this.#startNode);
		const typedArgs: UmbElementTreeChildrenOfRequestArgs = {
			...args,
			parent: { unique: this.#startNode?.unique ?? null, entityType: this.#startNode?.entityType ?? 'element' },
			foldersOnly: this.#folderOnly,
			dataType: this.#dataType,
		};
		return this.#tree.requestTreeItemsOf(typedArgs);
	};

	requestTreeItemAncestors = (args: UmbTreeAncestorsOfRequestArgs) => this.#tree.requestTreeItemAncestors(args);

	requestItems = (uniques: Array<string>) =>
		this.#folderOnly ? this.#folderItem.requestItems(uniques) : this.#elementItem.requestItems(uniques);

	treePickableFilter: (treeItem: UmbTreeItemModel) => boolean = (treeItem) => treeItem.isFolder === this.#folderOnly;
}
