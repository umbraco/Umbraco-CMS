import { getConfigValue } from '@umbraco-cms/backoffice/utils';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import {
	UmbDocumentItemRepository,
	UmbDocumentSearchRepository,
	UmbDocumentTreeRepository,
	UMB_DOCUMENT_ENTITY_TYPE,
} from '@umbraco-cms/backoffice/document';
import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '@umbraco-cms/backoffice/document-type';
import { UMB_PROPERTY_TYPE_BASED_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/content';
import type {
	UmbDocumentSearchItemModel,
	UmbDocumentSearchRequestArgs,
	UmbDocumentTreeChildrenOfRequestArgs,
	UmbDocumentTreeItemModel,
	UmbDocumentTreeRootItemsRequestArgs,
	UmbDocumentTreeRootModel,
} from '@umbraco-cms/backoffice/document';
import type {
	UmbPickerSearchableDataSource,
	UmbPickerTreeDataSource,
} from '@umbraco-cms/backoffice/picker-data-source';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import type { UmbTreeAncestorsOfRequestArgs } from '@umbraco-cms/backoffice/tree';

type ExampleDocumentPickerConfigCollectionModel = Array<
	{ alias: 'filter'; value: string } | { alias: 'startNodeId'; value: string }
>;

export class ExampleDocumentPickerPropertyEditorDataSource
	extends UmbControllerBase
	implements
		UmbPickerTreeDataSource<UmbDocumentTreeItemModel, UmbDocumentTreeRootModel>,
		UmbPickerSearchableDataSource<UmbDocumentSearchItemModel>
{
	#tree = new UmbDocumentTreeRepository(this);
	#item = new UmbDocumentItemRepository(this);
	#search = new UmbDocumentSearchRepository(this);
	#config: ExampleDocumentPickerConfigCollectionModel = [];

	async #getDataTypeUnique(): Promise<UmbReferenceByUnique | undefined> {
		const ctx = await this.getContext(UMB_PROPERTY_TYPE_BASED_PROPERTY_CONTEXT);
		return await this.observe(ctx?.dataType)?.asPromise();
	}

	treePickableFilter: (treeItem: UmbDocumentTreeItemModel) => boolean = (treeItem) => !!treeItem.unique;

	setConfig(config: ExampleDocumentPickerConfigCollectionModel) {
		// TODO: add examples for all config options
		this.#config = config;
		this.#applyPickableFilterFromConfig();
	}

	getConfig(): ExampleDocumentPickerConfigCollectionModel {
		return this.#config;
	}

	async requestTreeStartNode() {
		if (!this.#config?.length) return;
		const unique = getConfigValue(this.#config, 'startNodeId');
		return unique ? { unique, entityType: UMB_DOCUMENT_ENTITY_TYPE } : undefined;
	}

	requestTreeRoot() {
		return this.#tree.requestTreeRoot();
	}

	async requestTreeRootItems(args: UmbDocumentTreeRootItemsRequestArgs) {
		args.dataType = await this.#getDataTypeUnique();
		return this.#tree.requestTreeRootItems(args);
	}

	async requestTreeItemsOf(args: UmbDocumentTreeChildrenOfRequestArgs) {
		args.dataType = await this.#getDataTypeUnique();
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
		const filterString = getConfigValue(this.#config, 'filter');
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
