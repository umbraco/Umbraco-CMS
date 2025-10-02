import type { UmbPickerPropertyEditorTreeDataSource } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import {
	UmbMediaItemRepository,
	UmbMediaSearchRepository,
	UmbMediaTreeRepository,
} from '@umbraco-cms/backoffice/media';

export class UmbMediaPickerPropertyEditorDataSource
	extends UmbControllerBase
	implements UmbPickerPropertyEditorTreeDataSource
{
	#tree = new UmbMediaTreeRepository(this);
	#item = new UmbMediaItemRepository(this);
	#search = new UmbMediaSearchRepository(this);
	#config: any;

	setConfig(config: any): void {
		this.#config = config;
	}

	getConfig(): any {
		return this.#config;
	}

	requestTreeRoot(): Promise<any> {
		return this.#tree.requestTreeRoot();
	}

	requestTreeRootItems(args: any): Promise<any> {
		return this.#tree.requestTreeRootItems({
			skip: args.skip,
			take: args.take,
			paging: args.paging,
			foldersOnly: args.foldersOnly,
		});
	}

	requestTreeItemsOf(args: any): Promise<any> {
		return this.#tree.requestTreeItemsOf({
			parent: args.parent,
			skip: args.skip,
			take: args.take,
			paging: args.paging,
			foldersOnly: args.foldersOnly,
		});
	}

	requestTreeItemAncestors(args: any): Promise<any> {
		return this.#tree.requestTreeItemAncestors({ treeItem: args.treeItem });
	}

	requestItems(uniques: Array<string>): Promise<any> {
		return this.#item.requestItems(uniques);
	}

	requestSearch(args: any): Promise<any> {
		return this.#search.search(args);
	}
}

export { UmbMediaPickerPropertyEditorDataSource as api };
