import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import {
	UmbDocumentItemRepository,
	UmbDocumentSearchRepository,
	UmbDocumentTreeRepository,
	type UmbDocumentSearchItemModel,
	type UmbDocumentSearchRequestArgs,
	type UmbDocumentTreeItemModel,
	type UmbDocumentTreeRootModel,
} from '@umbraco-cms/backoffice/document';
import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '@umbraco-cms/backoffice/document-type';
import type {
	UmbPickerSearchableDataSource,
	UmbPickerTreeDataSource,
} from '@umbraco-cms/backoffice/picker-data-source';
import type { UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeRootItemsRequestArgs,
} from '@umbraco-cms/backoffice/tree';
import { getConfigValue, type UmbConfigCollectionModel } from '@umbraco-cms/backoffice/utils';

export class ExampleDocumentPickerPropertyEditorDataSource
	extends UmbControllerBase
	implements
		UmbPickerTreeDataSource<UmbDocumentTreeItemModel, UmbDocumentTreeRootModel>,
		UmbPickerSearchableDataSource<UmbDocumentSearchItemModel>
{
	#tree = new UmbDocumentTreeRepository(this);
	#item = new UmbDocumentItemRepository(this);
	#search = new UmbDocumentSearchRepository(this);
	#config: UmbConfigCollectionModel = [];

	treePickableFilter: (treeItem: UmbDocumentTreeItemModel) => boolean = (treeItem) => !!treeItem.unique;

	setConfig(config: UmbConfigCollectionModel) {
		this.#config = config;
		this.#applyPickableFilterFromConfig();
	}

	getConfig(): UmbConfigCollectionModel {
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
		const allowedContentTypes = this.#getAllowedDocumentTypesConfig();
		const combinedArgs: UmbDocumentSearchRequestArgs = { ...args, allowedContentTypes };

		return this.#search.search(combinedArgs);
	}

	#getAllowedDocumentTypesConfig() {
		const filterString = getConfigValue<string>(this.#config, 'filter');
		const filterArray = filterString ? filterString.split(',') : [];
		const allowedContentTypes: UmbDocumentSearchRequestArgs['allowedContentTypes'] = filterArray.map(
			(unique: string) => ({
				entityType: UMB_DOCUMENT_TYPE_ENTITY_TYPE,
				unique,
			}),
		);
		return allowedContentTypes;
	}

	#applyPickableFilterFromConfig() {
		const allowedDocumentTypes = this.#getAllowedDocumentTypesConfig();
		if (!allowedDocumentTypes || allowedDocumentTypes.length === 0) return;
		this.treePickableFilter = (treeItem: UmbDocumentTreeItemModel) =>
			allowedDocumentTypes.some((entityType) => entityType.unique === treeItem.documentType.unique);
	}
}

export { ExampleDocumentPickerPropertyEditorDataSource as api };
