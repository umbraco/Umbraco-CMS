import type { UmbPickerPropertyEditorTreeDataSource } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import {
	UmbMediaItemRepository,
	UmbMediaSearchRepository,
	UmbMediaTreeRepository,
} from '@umbraco-cms/backoffice/media';
import type { UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeRootItemsRequestArgs,
} from '@umbraco-cms/backoffice/tree';

export class UmbMediaPickerPropertyEditorDataSource
	extends UmbControllerBase
	implements UmbPickerPropertyEditorTreeDataSource
{
	#tree = new UmbMediaTreeRepository(this);
	#item = new UmbMediaItemRepository(this);
	#search = new UmbMediaSearchRepository(this);
	#config: any;

	setConfig(config: any) {
		this.#config = config;
	}

	getConfig() {
		return this.#config;
	}

	requestTreeRoot() {
		return this.#tree.requestTreeRoot();
	}

	requestTreeRootItems(args: UmbTreeRootItemsRequestArgs) {
		return this.#tree.requestTreeRootItems({
			skip: args.skip,
			take: args.take,
			paging: args.paging,
			foldersOnly: args.foldersOnly,
		});
	}

	requestTreeItemsOf(args: UmbTreeChildrenOfRequestArgs) {
		return this.#tree.requestTreeItemsOf({
			parent: args.parent,
			skip: args.skip,
			take: args.take,
			paging: args.paging,
			foldersOnly: args.foldersOnly,
		});
	}

	requestTreeItemAncestors(args: UmbTreeAncestorsOfRequestArgs) {
		return this.#tree.requestTreeItemAncestors({ treeItem: args.treeItem });
	}

	requestItems(uniques: Array<string>) {
		return this.#item.requestItems(uniques);
	}

	search(args: UmbSearchRequestArgs) {
		return this.#search.search(args);
	}
}

export { UmbMediaPickerPropertyEditorDataSource as api };
