import type { UmbPickerPropertyEditorTreeDataSource } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import {
	UmbDocumentItemRepository,
	UmbDocumentSearchRepository,
	UmbDocumentTreeRepository,
} from '@umbraco-cms/backoffice/document';
import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '@umbraco-cms/backoffice/document-type';
import type { UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeRootItemsRequestArgs,
} from '@umbraco-cms/backoffice/tree';

export class UmbDocumentPickerPropertyEditorDataSource
	extends UmbControllerBase
	implements UmbPickerPropertyEditorTreeDataSource
{
	#tree = new UmbDocumentTreeRepository(this);
	#item = new UmbDocumentItemRepository(this);
	#search = new UmbDocumentSearchRepository(this);
	#config: any;

	setConfig(config: any) {
		this.#config = config;
	}

	getConfig(): any {
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
		const filterString = this.#config?.find((x: any) => x.alias === 'filter')?.value;
		const filterArray = filterString ? filterString.split(',') : [];
		const allowedContentTypes = filterArray.map((x: string) => ({
			unique: x,
			entityType: UMB_DOCUMENT_TYPE_ENTITY_TYPE,
		}));

		const combinedArgs = { ...args, allowedContentTypes };

		return this.#search.search(combinedArgs);
	}
}

export { UmbDocumentPickerPropertyEditorDataSource as api };
