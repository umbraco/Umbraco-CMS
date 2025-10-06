import type {
	UmbPickerPropertyEditorTreeDataSource,
	UmbPropertyEditorDataSourceConfigModel,
	UmbPickerPropertyEditorSearchableDataSource,
} from '../../src/packages/core/property-editor/property-editor-data-source/types.js';
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

export class ExampleMediaPickerPropertyEditorDataSource
	extends UmbControllerBase
	implements UmbPickerPropertyEditorTreeDataSource, UmbPickerPropertyEditorSearchableDataSource
{
	#tree = new UmbMediaTreeRepository(this);
	#item = new UmbMediaItemRepository(this);
	#search = new UmbMediaSearchRepository(this);
	#config: UmbPropertyEditorDataSourceConfigModel = [];

	setConfig(config: UmbPropertyEditorDataSourceConfigModel) {
		this.#config = config;
	}

	getConfig(): UmbPropertyEditorDataSourceConfigModel {
		return this.#config;
	}

	requestTreeRoot() {
		return this.#tree.requestTreeRoot();
	}

	requestTreeRootItems(args: UmbTreeRootItemsRequestArgs) {
		return this.#tree.requestTreeRootItems(args);
	}

	requestTreeItemsOf(args: UmbTreeChildrenOfRequestArgs) {
		return this.#tree.requestTreeItemsOf(args);
	}

	requestTreeItemAncestors(args: UmbTreeAncestorsOfRequestArgs) {
		return this.#tree.requestTreeItemAncestors(args);
	}

	requestItems(uniques: Array<string>) {
		return this.#item.requestItems(uniques);
	}

	search(args: UmbSearchRequestArgs) {
		return this.#search.search(args);
	}
}

export { ExampleMediaPickerPropertyEditorDataSource as api };
