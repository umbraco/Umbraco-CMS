import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import {
	UmbDocumentItemRepository,
	UmbDocumentSearchRepository,
	UmbDocumentTreeRepository,
	type UmbDocumentSearchRequestArgs,
} from '@umbraco-cms/backoffice/document';
import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '@umbraco-cms/backoffice/document-type';
import type {
	UmbPickerPropertyEditorSearchableDataSource,
	UmbPickerPropertyEditorTreeDataSource,
} from '@umbraco-cms/backoffice/picker-property-editor';
import type { UmbPropertyEditorDataSourceConfigModel } from '@umbraco-cms/backoffice/property-editor';
import type { UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeRootItemsRequestArgs,
} from '@umbraco-cms/backoffice/tree';

export class ExampleDocumentPickerPropertyEditorDataSource
	extends UmbControllerBase
	implements UmbPickerPropertyEditorTreeDataSource, UmbPickerPropertyEditorSearchableDataSource
{
	#tree = new UmbDocumentTreeRepository(this);
	#item = new UmbDocumentItemRepository(this);
	#search = new UmbDocumentSearchRepository(this);
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
		const filterString = this.#config?.find((x) => x.alias === 'filter')?.value as string;
		const filterArray = filterString ? filterString.split(',') : [];
		const allowedContentTypes: UmbDocumentSearchRequestArgs['allowedContentTypes'] = filterArray.map(
			(unique: string) => ({
				entityType: UMB_DOCUMENT_TYPE_ENTITY_TYPE,
				unique,
			}),
		);

		const combinedArgs: UmbDocumentSearchRequestArgs = { ...args, allowedContentTypes };

		return this.#search.search(combinedArgs);
	}
}

export { ExampleDocumentPickerPropertyEditorDataSource as api };
